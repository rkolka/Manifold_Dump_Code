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
        String extnet = @"D:\rdn\Bin\manifold-9.0.165.5-x64\bin64\ext.dll";
        using (M.Root root = new M.Root(extnet))
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
