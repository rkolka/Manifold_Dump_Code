using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Manifold;

static class Test
{
    [STAThread] // important
    static void Main()
    {
        // Path to ext.dll
        // If path is an empty string, the object will try to use EXT.DLL 
        // in the default installation path C:\Program Files\Manifold\v9.0
        String extdll = @"";  
        using (M.Root root = new M.Root(extdll))
        {
            Script.App = root.Application;
            Console.WriteLine(Script.App.Name);
            String mapfile = Path.GetFullPath(@"testmap.map");
            M.Database db = Script.App.CreateDatabaseForFile(mapfile, true);
            String filedir = Path.GetDirectoryName(mapfile);
            String filenamePrefix = Path.GetFileNameWithoutExtension(mapfile);
            Script.DumpDatabaseCode(db, filedir, filenamePrefix);
        }


    }
}
