// C#
// $reference: System.Core.dll

//copy "$(TargetDir)$(TargetName).dll" C:\Path\To\manifold-9\shared\Addins\

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
    private static String Indent = "\t";

    static void Main()
    {
        DateTime date = DateTime.Now;
        if (Manifold != null)
            App = Manifold.Application;
        App.OpenLog();
        using (M.Database db = App.GetDatabaseRoot())
        {

            String filedir = Path.GetDirectoryName(MapFilePath(db));
            String filenamePrefix = Path.GetFileNameWithoutExtension(MapFilePath(db));
            DumpDatabaseCode(db, filedir, filenamePrefix);
            App.Log(String.Format(@"Dumps saved: {0}\{1}.*", filedir, filenamePrefix));
        }
    }

    public static void DumpDatabaseCode(M.Database db, String filedir, String filenamePrefix)
    {
        String filename;

        filename = String.Format(@"{0}\{1}.components.txt", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCompNames(db));
        //App.Log("Component list saved: " + filename);

        filename = String.Format(@"{0}\{1}.cleanup.sql", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCleanupStatements(db));
        //App.Log("Cleanup sql saved: " + filename);

        filename = String.Format(@"{0}\{1}.drop.sql", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpDropStatements(db));
        //App.Log("Drops sql saved: " + filename);


        filename = String.Format(@"{0}\{1}.create.sql", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCreateStatements(db));
        //App.Log("Create sql saved: " + filename);

        filename = String.Format(@"{0}\{1}.code.txt", filedir, filenamePrefix);
        File.WriteAllText(filename, DumpCodeAsText(db));
        //App.Log("SQL and script text saved: " + filename);
    }

    private static string DropStatement(String type, String name)
    {
        return String.Format("DROP {0} [{1}]; ", type, name);
    }




    static String DumpCodeAsText(M.Database db)
    {
        List<String> names = Names(db);
        StringBuilder builderQueries = new StringBuilder();
        StringBuilder builderScripts = new StringBuilder();
        foreach (String name in names)
        {
            String type = db.GetComponentType(name);

            if (type == "query")
                builderQueries.Append(ReportQuery(db, name));
            else if (type == "script")
                builderScripts.Append(ReportScript(db, name));
        }
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));
        builder.Append(builderQueries.ToString());
        builder.Append(builderScripts.ToString());
        return builder.ToString();

    }

    private static string MapFilePath(M.Database db)
    {
        M.PropertySet dbConnProps = App.CreatePropertySetParse(db.Connection);
        string path = dbConnProps.GetProperty("Source");
        return path;
    }

    static String DumpCreateStatements(M.Database db)
    {
        // collect component names
        List<String> names = Names(db);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));

        // report components
        foreach (String name in names)
        {
            String checkFolder = db.GetProperty(name, "Folder");
            if (!checkFolder.StartsWith("System Data"))
            {
                String typeUpper = db.GetComponentType(name).ToUpper();

                builder.AppendLine(String.Format("---- {0}: {1}", typeUpper, name));
                builder.AppendLine(String.Format("--DROP {0} [{1}];", typeUpper, name));

                if (typeUpper == "TABLE")
                {
                    builder.Append(DumpTableCreate(db, name));
                    builder.AppendLine();

                }
                else if (typeUpper == "DATASOURCE" | typeUpper == "MAP" | typeUpper == "DRAWING" | typeUpper == "LABELS" | typeUpper == "LAYOUT" | typeUpper == "IMAGE")
                {
                    builder.Append(DumpOtherCreate(db, name));
                    builder.AppendLine();
                }

            }
        }
        return builder.ToString();
    }

    static String DumpTableCreate(M.Database db, String name)
    {
        List<String> itemList = new List<string>();
        using (M.Table table = db.Search(name))
        {
            M.Schema schema = table.GetSchema();
            M.Schema.FieldSet fieldSet = schema.Fields;
            M.Schema.IndexSet indexSet = schema.Indexes;
            M.Schema.ConstraintSet constraintSet = schema.Constraints;
            M.PropertySet propertySet = db.GetProperties(name);

            itemList.AddRange(FieldSubClauseList(fieldSet));
            itemList.AddRange(IndexSubClauseList(indexSet));
            //schemaItems.AddRange(ConstraintSubClauseList(constraintSet));
            itemList.AddRange(PropertySubClauseList(propertySet));
        }
        String items = String.Join("," + Environment.NewLine, itemList.Select(i => String.Concat(Indent, i)));

        String typeUpper = db.GetComponentType(name).ToUpper();

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(CreateStatement(typeUpper, name, items));
        builder.AppendLine();
        return builder.ToString();
    }


    private static List<String> IndexSubClauseList(M.Schema.IndexSet indexSet)
    {
        List<String> ixs = new List<String>();
        foreach (M.Schema.Index ix in indexSet)
        {
            String fields = String.Join(", ", IndexFieldsList(ix.Fields));
            ixs.Add(IndexSubClause(ix.Name, ix.Type.ToUpper(), fields));
        }
        return ixs;
    }

    private static List<String> IndexFieldsList(M.Schema.IndexFieldSet indexFieldSet)
    {
        List<String> ixfs = new List<String>();
        foreach (M.Schema.IndexField ixf in indexFieldSet)
        {
            String options = String.Join(" ", IndexFieldOptionsList(ixf));
            ixfs.Add(IndexFieldSubClause(ixf.Name, options));
        }
        return ixfs;
    }
    private static String IndexSubClause(String name, String type, String fields)
    {
        String ix = String.Format("INDEX [{0}] {1} ({2})", name, type, fields);
        return ix;
    }

    private static String IndexFieldSubClause(String name, String options)
    {
        String ixf = String.Format("[{0}]", name);
        if (options.Length > 0)
            ixf = String.Concat(ixf, " ", options);
        return ixf;
    }

    private static List<String> IndexFieldOptionsList(M.Schema.IndexField ixf)
    {
        List<String> optionList = new List<String>();
        if (ixf.Collation.Length > 0) optionList.Add(String.Format("COLLATE '{0}'", ixf.Collation));
        if (ixf.IgnoreCase) optionList.Add("NOCASE");
        if (ixf.IgnoreAccent) optionList.Add("NOACCENT");
        if (ixf.IgnoreSymbols) optionList.Add("NOSYMBOLS");
        if (ixf.Descending) optionList.Add("DESC");

        if (ixf.TileReduce.Length > 0) optionList.Add(String.Format("TILEREDUCE '{0}'", ixf.TileReduce.ToUpper()));
        if (ixf.TileSize.Length > 0) optionList.Add(String.Format("TILESIZE ({0})", ixf.TileSize));
        if (ixf.TileType.Length > 0) optionList.Add(String.Format("TILETYPE {0}", ixf.TileType.ToUpper()));
        return optionList;
    }






    private static List<String> FieldSubClauseList(M.Schema.FieldSet fieldSet)
    {
        List<String> fs = new List<String>();
        foreach (M.Schema.Field f in fieldSet)
            fs.Add(FieldSubClause(f.Name, f.Type.ToUpper(), f.Expression));
        return fs;
    }

    private static String FieldSubClause(String name, String type, String expression = "")
    {
        String f = String.Format("[{0}] {1}", name, type);
        if (expression.Length > 0)
            f = String.Concat(f, " AS ", expression);
        return f;
    }




    private static String PropertySubClause(String name, String data)
    {
        String dataEscaped = data.Replace(@"\", @"\\").Replace(@"'", @"\'");
        return String.Format("PROPERTY '{0}' '{1}'", name, dataEscaped);
    }

    private static List<String> PropertySubClauseList(M.PropertySet propSet)
    {
        List<String> ps = new List<String>();
        foreach (M.Property p in propSet)
            ps.Add(PropertySubClause(p.Name, p.Data));
        return ps;
    }

    private static String CreateStatement(String type, String name, String items)
    {
        return String.Format("CREATE {1} [{2}] ({0}{3}{0});", Environment.NewLine, type, name, items);
    }



    private static String DumpOtherCreate(M.Database db, string name)
    {
        String typeUpper = db.GetComponentType(name).ToUpper();

        M.PropertySet props = db.GetProperties(name);
        List<String> itemsList = PropertySubClauseList(props);
        String itemsText = String.Join("," + Environment.NewLine, itemsList.Select(i => String.Concat(Indent, i)));

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(CreateStatement(typeUpper, name, itemsText));
        builder.AppendLine();
        return builder.ToString();

    }



    private static string DumpDropStatements(M.Database db)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));
        foreach (String name in Names(db))
        {
            String checkFolder = db.GetProperty(name, "Folder");
            if (!checkFolder.StartsWith("System Data"))
            {
                String type = db.GetComponentType(name);
                builder.AppendLine(DropStatement(type, name));
            }
        }
        return builder.ToString();

    }

    private static string CleanupStatement(String name)
    {
        return String.Format("DELETE FROM [{0}]; ", name);
    }

    private static string DumpCleanupStatements(M.Database db)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));
        foreach (String name in Names(db))
        {
            String type = db.GetComponentType(name);
            String checkFolder = db.GetProperty(name, "Folder");
            if (!checkFolder.StartsWith("System Data") & type == "table")
                builder.AppendLine(CleanupStatement(name));
        }
        return builder.ToString();

    }

    private static string DumpCompNames(M.Database db)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- Mapfile: " + MapFilePath(db));
        foreach (String s in CompNames(db))
            builder.AppendLine("-- " + s);
        return builder.ToString();

    }



    static String ReportQuery(M.Database db, String name)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- query: " + name);
        builder.AppendLine();
        builder.AppendLine(db.GetProperty(name, "text"));
        builder.AppendLine();
        return builder.ToString();
    }

    static String ReportScript(M.Database db, String name)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("---- script: " + name);
        builder.AppendLine();
        builder.AppendLine(db.GetProperty(name, "text"));
        builder.AppendLine();
        return builder.ToString();
    }



    static List<String> Names(M.Database db)
    {
        List<String> names = new List<String>();
        using (M.Table root = db.Search("mfd_root"))
        {
            using (M.Sequence sequence = root.SearchAll(new String[] { "name" }))
            {
                while (sequence.Fetch())
                    names.Add(sequence.GetValues()[0].Data.ToString());
            }
        }
        names.Sort();
        return names;
    }

    static List<String> CompNames(M.Database db)
    {
        List<String> names = new List<String>();
        using (M.Table root = db.Search("mfd_root"))
        {
            using (M.Sequence sequence = root.SearchAll(new String[] { "mfd_id", "type", "name" }))
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
