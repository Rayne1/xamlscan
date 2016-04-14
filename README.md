# xamlscan
Scanner of xaml files

Tool intented to inspect xaml files inside WPD application.
Main purpose is to get a clear picture of all xaml files (controls or resource dictionaries) and their relations.
The idea is to create static xaml file analyzer.

Abilities:
  - inspects view boxes and presents them
  - inspect colors and brushes
  - builds resource dictionaries and views relation
  
Plans:
  - present persentage of dictionary resources usage
  - Detect when dictionary is newer used but merged
  - Detect if exists same colors but with different names
  - Detect resource name duplication
  - Present list of not merged dictionaries
  - Present list of newer used resources
  - Fuzzy search of resource
  - Fuzzy search of dictionary
  - Open file in VisualStudio
  - Present number of merges of the selected dictionary
