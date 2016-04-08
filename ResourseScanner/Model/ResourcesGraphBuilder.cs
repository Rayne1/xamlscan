using Graphviz4Net.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Diagnostics;
using Graphviz4Net.Dot;
using ResourseScanner.Model;

namespace ResourseScanner
{
    public class ResourcesGraphBuilder
    {
        HashSet<GraphNodeViewModelBase> Nodes = new HashSet<GraphNodeViewModelBase>();

        public Dictionary<string, List<DictionaryNodeViewModel>> PendingMerges { get; private set; }

        class CurrentFileInfo
        {
            public string Name { get; set; }

            public int ChildsCnt { get; set; }

            public void Init(string name)
            {
                Name = name;
                ChildsCnt = 0;
            }
        }

        public Graph<GraphNodeViewModelBase> Graph { get; private set; }

        private CurrentFileInfo CurrentFile { get; set; }

        private DictionaryNodeViewModel CurretFileNode { get; set; }

        public ResourcesGraphBuilder()
        {
            CurrentFile = new CurrentFileInfo();

            PendingMerges = new Dictionary<string, List<DictionaryNodeViewModel>>();

            Reset();
            //Graph.Attributes.Add();
            //Graph.Attributes.Add()
        }

        public DictionaryNodeViewModel AddFile(string path)
        {
            CurrentFile.Init(path);

            CurretFileNode = new DictionaryNodeViewModel(path);

            Nodes.Add(CurretFileNode);
            Graph.AddVertex(CurretFileNode);

            /*Check pending merges*/

            foreach (var item in PendingMerges)
                if (path.EndsWith(item.Key))
                {
                    foreach (var f in item.Value)
                    {
                        Dictionary<string, string> attributes = new Dictionary<string, string>();
                        var edge = new Edge<GraphNodeViewModelBase>(CurretFileNode, f, new DiamondArrow(), null, null, null, attributes);
                        Graph.AddEdge(edge);
                        f.MergedFiles.Add(CurretFileNode);
                        CurretFileNode.IsMerged = true;
                        f.RefreshMetrics();
                    }

                    PendingMerges.Remove(item.Key);
                    break;
                }

            return CurretFileNode;
        }

        public void AddResourceToCurrentFile(DictionaryEntry entry)
        {
            var entryNode = new ResourceNodeViewModel(entry);
            Nodes.Add(entryNode);
            CurretFileNode.Childs.Add(entryNode);
            CurretFileNode.RefreshMetrics();
            Graph.AddVertex(entryNode);
            var edge = new Edge<GraphNodeViewModelBase>(CurretFileNode, entryNode);// fillcolor
            //edge.Attributes.Add("color", "blue");
            //edge.Attributes.Add("arrowhead", "diamond");
            Graph.AddEdge(edge);
        }

        public void AddResourceToCurrentFile(string key)
        {
            CurretFileNode.Keys.Add(key);
            CurretFileNode.RefreshMetrics();
        }

        public void Reset()
        {
            Graph = new Graph<GraphNodeViewModelBase>();
            Graph.Attributes.Add("overlap", "false");
            Nodes.Clear();
            PendingMerges.Clear();
            //Graph.Attributes.Add(
            //"edge","[color = maroon]");
        }

        public void AddReferenceToAnotherFile(string uri)
        {
            string localPath = String.Empty;

            /* check if file already in the graph */
            int lps = uri.IndexOf(";component");

            if (lps < 0)
            {
                /*handle local references.*/
                localPath = Path.Combine(Path.GetDirectoryName(CurretFileNode.FilePath), uri.Replace('/', '\\'));
            }
            else {

                /*handle relative uris*/
                localPath = uri.Substring(lps + 10).Replace('/', '\\');
                localPath = uri.Substring(0, lps).Replace('/', '\\') + localPath;
            }

            var node = Nodes.OfType<DictionaryNodeViewModel>().Where(n => System.IO.Path.GetFileName(uri) == n.FileName
                                        && n.FilePath.EndsWith(localPath)
                                    ).SingleOrDefault();

            if (node != null)
            {
                var edge = new Edge<GraphNodeViewModelBase>(CurretFileNode, node, null, new DiamondArrow());
                Graph.AddEdge(edge);
                CurretFileNode.MergedFiles.Add(node);
                node.IsMerged = true;
                return;
            }

            /* Add pending merging */
            if (PendingMerges.Keys.Contains(localPath))
                PendingMerges[localPath].Add(CurretFileNode);
            else
                PendingMerges.Add(localPath, new List<DictionaryNodeViewModel>() { CurretFileNode });
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(" -- Not merged dictionaries --");
            sb.AppendLine();
            int cnt = 0;
            foreach (var item in Nodes.OfType<DictionaryNodeViewModel>().Where(n=>!n.IsMerged && n.IsDictionary))
            {
                sb.AppendLine(item.FilePath);
                cnt++;
            }

            sb.AppendLine().AppendFormat(" -- total count : {0}", cnt).AppendLine();

            return sb.ToString();
        }
    }
}
