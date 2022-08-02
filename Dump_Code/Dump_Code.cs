// C#
// $reference: System.Core.dll

// In Debug mode 
// * Builds into C:\Program Files\Manifold\v9.0\extras\Debug\Dump_Code\ 
// * Starts C:\Program Files\Manifold\v9.0\bin64\manifold.exe
// * Breakpoints can be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using M = Manifold;



public class Script
{
        

    static M.Context Manifold;
    public static M.Application App;
    private static readonly string Indent = "  ";

    static void Main()
    {
        DateTime date = DateTime.Now;
        if (Manifold != null)
            App = Manifold.Application;
        App.OpenLog();
        using (M.Database db = App.GetDatabaseRoot())
        {

            string filedir = Path.GetDirectoryName(MapFilePath(db));
            string filenamePrefix = Path.GetFileNameWithoutExtension(MapFilePath(db));
            DumpDatabaseCode(db, filedir, filenamePrefix);
            App.Log(String.Format(@"Dumps saved: {0}\{1}.*", filedir, filenamePrefix));
        }
    }

    public static void DumpDatabaseCode(M.Database db, string filedir, string filenamePrefix)
    {
        string filename;

        filename = String.Format(@"{0}\{1}.components.txt", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCompNames(db), Encoding.UTF8);

        filename = String.Format(@"{0}\{1}.cleanup.sql", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCleanupStatements(db), Encoding.UTF8);

        filename = String.Format(@"{0}\{1}.drop.sql", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpDropStatements(db), Encoding.UTF8);

        filename = String.Format(@"{0}\{1}.create.sql", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCreateStatements(db), Encoding.UTF8);
    }

    static string MapFilePath(M.Database db)
    {
        M.PropertySet dbConnProps = App.CreatePropertySetParse(db.Connection);
        string path = dbConnProps.GetProperty("Source");
        return path;
    }

    static string DumpCreateStatements(M.Database db)
    {
        // collect component names
        List<string> names = Names(db);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));

        string systemFolder = db.GetProperty("mfd_meta", "Folder");

        // report components
        foreach (string name in names)
        {
            string checkFolder = db.GetProperty(name, "Folder");
            if (!checkFolder.Equals(systemFolder))
            {
                string type = db.GetComponentType(name).ToUpper();

                builder.AppendLine(String.Format("---- {0}: {1}", type, name));
                builder.AppendLine(String.Format("--{0}", DropStatement(type, name)));

                string body = CreateStatementBody(db, name);
                builder.Append(CreateStatement(name, type, body));
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    static string DropStatement(string type, string name)
    {
        return String.Format("DROP {0} [{1}];", type, name);
    }

    static string CleanupStatement(string name)
    {
        return String.Format("DELETE FROM [{0}]; ", name);
    }

    static string CreateStatement(string name, string type, string body)
    {
        return String.Format("CREATE {1} [{0}] ({3}{2}{3});", name, type, body, Environment.NewLine);
    }

    static string FieldItem(string name, string type, string context = "", string expression = "")
    {
        if (context.Length > 0)
            context = string.Format(" WITH [[ {0} ]]", context);
        if (expression.Length > 0)
            expression = string.Format(" AS [[ {0} ]]", expression);
        string f = string.Format("[{0}] {1}{2}{3}", name, type, context, expression);
        return f;
    }

    static string IndexItem(string name, string type, string fields)
    {
        string ix = String.Format("INDEX [{0}] {1} ({2})", name, type, fields);
        return ix;
    }

    static string ConstraintItem(string name, string context, string expression)
    {
        if (context.Length > 0)
            context = String.Format(" WITH [[ {0} ]]", context);
        string c = String.Format("CONSTRAINT [{0}]{1} AS [[ {2} ]]", name, context, expression);
        return c;
    }

    static string PropertyItem(string name, string data)
    {
        string dataEscaped = data.Replace(@"\", @"\\").Replace(@"'", @"\'");
        return String.Format("PROPERTY '{0}' '{1}'", name, dataEscaped);
    }

    static string CreateStatementBody(M.Database db, string name)
    {
        List<string> items = new List<string>();

        string type = db.GetComponentType(name).ToUpper();

        if (type == "TABLE")
        {
            using (M.Table table = db.Search(name))
            {
                M.Schema schema = table.GetSchema();

                items.AddRange(FieldItems(schema.Fields));
                items.AddRange(IndexItems(schema.Indexes));
                items.AddRange(ConstraintItems(schema.Constraints));
            }
        }

        M.PropertySet propertySet = db.GetProperties(name);
        items.AddRange(PropertyItems(propertySet));
        
        string body = String.Join("," + Environment.NewLine, items.Select(i => String.Concat(Indent, i)));
        return body;
    }

    private static List<string> FieldItems(M.Schema.FieldSet fieldSet)
    {
        List<string> fs = new List<string>();
        foreach (M.Schema.Field f in fieldSet)
            fs.Add(FieldItem(f.Name, f.Type.ToUpper(), f.ExpressionContext, f.Expression));
        return fs;
    }

    private static List<string> IndexItems(M.Schema.IndexSet indexSet)
    {
        List<string> ixs = new List<string>();
        foreach (M.Schema.Index ix in indexSet)
        {
            string fields = String.Join(", ", IndexFields(ix.Fields));
            ixs.Add(IndexItem(ix.Name, ix.Type.ToUpper(), fields));
        }
        return ixs;
    }

    private static List<string> ConstraintItems(M.Schema.ConstraintSet constraintSet)
    {
        List<string> cs = new List<string>();
        foreach (M.Schema.Constraint c in constraintSet)
        {
            cs.Add(ConstraintItem(c.Name, c.ExpressionContext, c.Expression));
        }
        return cs;
    }

    private static List<string> PropertyItems(M.PropertySet propSet)
    {
        List<string> ps = new List<string>();
        foreach (M.Property p in propSet)
            ps.Add(PropertyItem(p.Name, p.Data));
        return ps;
    }

    private static List<string> IndexFields(M.Schema.IndexFieldSet indexFieldSet)
    {
        List<string> ixfs = new List<string>();
        foreach (M.Schema.IndexField ixf in indexFieldSet)
        {
            string options = String.Join(" ", IndexFieldOptions(ixf));
            ixfs.Add(IndexFieldSubClause(ixf.Name, options));
        }
        return ixfs;
    }

    static string IndexFieldSubClause(string name, string options)
    {
        string ixf = String.Format("[{0}] {1}", name, options).Trim(' ');
        return ixf;
    }

    private static List<string> IndexFieldOptions(M.Schema.IndexField ixf)
    {
        List<string> options = new List<string>();
        if (ixf.Collation.Length > 0) options.Add(String.Format("COLLATE '{0}'", ixf.Collation));
        if (ixf.IgnoreCase) options.Add("NOCASE");
        if (ixf.IgnoreAccent) options.Add("NOACCENT");
        if (ixf.IgnoreSymbols) options.Add("NOSYMBOLS");
        if (ixf.Descending) options.Add("DESC");

        if (ixf.TileReduce.Length > 0) options.Add(String.Format("TILEREDUCE '{0}'", ixf.TileReduce.ToUpper()));
        if (ixf.TileSize.Length > 0) options.Add(String.Format("TILESIZE ({0})", ixf.TileSize));
        if (ixf.TileType.Length > 0) options.Add(String.Format("TILETYPE {0}", ixf.TileType.ToUpper()));
        return options;
    }

    static string DumpDropStatements(M.Database db)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));

        string systemFolder = db.GetProperty("mfd_meta", "Folder");

        foreach (string name in Names(db))
        {
            string checkFolder = db.GetProperty(name, "Folder");
            if (!checkFolder.Equals(systemFolder))
            {
                string type = db.GetComponentType(name).ToUpper();
                builder.AppendLine(DropStatement(type, name));
            }
        }
        return builder.ToString();

    }

    static string DumpCleanupStatements(M.Database db)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));
        
        string systemFolder = db.GetProperty("mfd_meta", "Folder");

        foreach (string name in Names(db))
        {
            string type = db.GetComponentType(name);
            string checkFolder = db.GetProperty(name, "Folder");
            if (!checkFolder.Equals(systemFolder) & type == "table")
                builder.AppendLine(CleanupStatement(name));
        }
        return builder.ToString();
    }

    static string DumpCompNames(M.Database db)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));
        foreach (string s in CompNames(db))
            builder.AppendLine("-- " + s);
        return builder.ToString();

    }

    static List<string> Names(M.Database db)
    {
        List<string> names = new List<string>();
        using (M.Table root = db.Search("mfd_root"))
        {
            using (M.Sequence sequence = root.SearchAll(new string[] { "name" }))
            {
                while (sequence.Fetch())
                    names.Add(sequence.GetValues()[0].Data.ToString());
            }
        }
        names.Sort();
        return names;
    }

    static List<string> CompNames(M.Database db)
    {
        var names = new List<string>();
        using (M.Table t = db.Search("mfd_root"))
        {
            using (M.Sequence sequence = t.SearchAll(new string[] { "mfd_id", "type", "name" }))
            {
                while (sequence.Fetch())
                {
                    names.Add(
                        sequence.GetValues()[1].Data.ToString()
                        + " " + "[" + sequence.GetValues()[2].Data.ToString() + "]"
                        + " " + sequence.GetValues()[0].Data.ToString()
                    );
                }
            }
        }
        names.Sort();
        return names;
    }
}
