using Graphviz4Net.Graphs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ResourseScanner.Model
{
    public class ViewsGraphBuilder
    {
        public Graph<GraphNodeViewModelBase> Graph { get; private set; }

        public ViewNodeViewModel Current { get; private set; }

        readonly List<ViewNodeViewModel> nodes = new List<ViewNodeViewModel>();

        public List<ViewNodeViewModel> Nodes
        {
            get
            {
                return nodes;
            }
        }

        public ViewsGraphBuilder()
        {
            Reset();
        }


        public ViewNodeViewModel AddView(string name, string fileName)
        {
            ViewNodeViewModel node = new ViewNodeViewModel(name, fileName);
            Current = node;
            Graph.AddVertex(node);
            Nodes.Add(node);

            return node;
        }

        internal void FormAdges()
        {
            //TODO: can do in parallel.

            for (int i = 0; i < Nodes.Count; i++)
            {
                string text = File.ReadAllText(Nodes[i].Path);
                var m = Regex.Match(text, @"d:DataContext=""{d:DesignInstance Type={x:Type (?:\w*:)(.*)}[\w= ,]*}""");
                if (m.Success)
                {
                    var vmVertex = new ViewModelNodeViewModel(m.Groups[1].Value);
                    Graph.AddVertex(vmVertex);
                    Edge<GraphNodeViewModelBase> edge = new Edge<GraphNodeViewModelBase>(vmVertex, Nodes[i], new DiamondArrow());
                    Graph.AddEdge(edge);
                }
                
                for (int j = 0; j < Nodes.Count; j++)
                {
                    if (i == j) continue;
                    m = Regex.Match(text, ":"+Nodes[j].Name + @"[\s>/]");
                    if (m.Success)
                    {

                        Nodes[i].ChilsViews.Add(Nodes[j]);
                        Edge<GraphNodeViewModelBase> edge = new Edge<GraphNodeViewModelBase>(Nodes[j], Nodes[i], new DiamondArrow());
                        Graph.AddEdge(edge);
                    }
                }
            }
        }

        internal void Reset()
        {
            Graph = new Graph<GraphNodeViewModelBase>();
            Graph.Attributes.Add("overlap", "false");
            Graph.Attributes.Add("rankdir", "RL");
            Nodes.Clear();
        }
    }
}
