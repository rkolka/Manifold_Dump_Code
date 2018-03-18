# Dump Code - Add-in for Manifold Release 9 
Manifold Release 9 Add-in 
Dumps code and DROP/CREATE scripts from Manifold project into textfiles in the same directory as project file.


* Adapted from http://www.georeference.org/forum/t142430.22#142437
* Builds with Visual Studio 2017 (Community Edition)
  - Builds into Dump_Code.dll 
  - Copy Dump_Code.dll under \<manifold\>\shared\\<subdir\>\  
* Uses Newtonsoft.Json.dll as reference.
  - this is automatically retrieved if built with VS
  - Copy Newtonsoft.Json.dll also under \<manifold\>\shared\\<subdir\>\
  
* Simply putting Dump_Code.cs into \<manifold\>\shared\ works also
  - for me gives error if Newtonsoft.Json.dll is under \<manifold\>\shared\\<subdir\>\
  - works for me if Newtonsoft.Json.dll is in \<manifold\>\bin64\
