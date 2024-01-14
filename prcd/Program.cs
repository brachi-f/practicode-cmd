
using System.CommandLine;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Channels;

var langsDict = new Dictionary<string, string>
{
    { "c#",".cs" },
    { "c",".c" },
    { "c++",".cpp" },
    { "java",".java" },
    { "javascript",".js" },
    { "typescript",".ts" },
    { "html",".html" },
    { "css",".css" },
    { "python",".py" }
};
var bundleCommand = new Command("bundle", "bundle code files to a single file");

//options
var languageOption = new Option<string>("language", "A list of file extensions to include in the file")
    .FromAmong("all", "c#", "c", "c++", "java", "javascript", "typescript", "html", "css", "python");
languageOption.IsRequired = true;
languageOption.AddAlias("-lang");
languageOption.AddAlias("-l");

var outputOption = new Option<FileInfo>("output", "Path to file") { IsRequired = true };
outputOption.AddAlias("-o");

var noteOption = new Option<bool>("note", "Whether to list the source code as a comment in the file");
noteOption.SetDefaultValue(false);
noteOption.AddAlias("-n");

var sortOption = new Option<string>("sort", "The order of copying the code files, according to the alphabet of the file name or according to the type of code")
    .FromAmong("name", "kind");
sortOption.SetDefaultValue("name");
sortOption.AddAlias("-s");

var removeEmptyLinesOption = new Option<bool>("remove-empty-lines", "Do delete empty lines");
removeEmptyLinesOption.SetDefaultValue(false);
removeEmptyLinesOption.AddAlias("-rel");
removeEmptyLinesOption.AddAlias("-r");

var authorOption = new Option<string>("author", "Registering the name of the creator of the file");
authorOption.AddAlias("-a");

bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((output, languages, note, sort, author, remove) =>
{
    string[] extentions = GetExtentions(languages);

    List<string> allfiles = Directory.GetDirectories(output.DirectoryName).ToList();

    List<FileInfo> textFiles = new List<FileInfo>();
    FileInfo info;
    allfiles.ForEach(f =>
    {

        info = new FileInfo(f);
        if (!(info.Name.Equals("bin") || info.Name.Equals("obj") || info.Name.Equals("debug") || info.Name.Equals("node_modules")))
        {
            Directory.GetFiles(f).ToList().ForEach(file =>
            {
                if (extentions.Contains(new FileInfo(file).Extension))
                    textFiles.Add(new FileInfo(file));
            });
            Console.WriteLine();
        }

    });
    Console.WriteLine("text files");
    foreach (var item in textFiles)
        Console.Write(item.FullName + ", ");
    Console.WriteLine();
    if (sort.Equals("kind"))
        textFiles = textFiles.OrderBy(f => f.Extension).ToList();
    else
        textFiles = textFiles.OrderBy(f => f.Name).ToList();
    try
    {
        var bundleFile = File.CreateText(output.FullName);
        Console.WriteLine($"file {output.Name} was created in {output.DirectoryName}");
        bundleFile.WriteLine(author);
        textFiles.ForEach(fi =>
        {
            if (note)
                bundleFile.WriteLine(@"// " + fi.FullName);
            List<string> lines;
            if (remove)
                lines = File.ReadAllLines(fi.FullName).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            else
                lines = File.ReadAllLines(fi.FullName).ToList();
            foreach (var line in lines)
                bundleFile.WriteLine(line);
        });
        bundleFile.Close();

    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: File path is invalid");
    }

}, outputOption, languageOption, noteOption, sortOption, authorOption, removeEmptyLinesOption);

var createRspCommand = new Command("create-rsp", "create respond file for bundle command");

createRspCommand.SetHandler(() =>
{
    string output;
    Console.WriteLine("enter path and file name: ");
    output = Console.ReadLine();
    string languages = "", answer = "";
    while (!answer.Equals("y") && !answer.Equals("n") && !answer.Equals("Y") && !answer.Equals("N"))
    {
        Console.WriteLine("Are you interested in including code files in all languages? (y/n)");
        answer = Console.ReadLine();
    }
    if (answer.Equals("n") || answer.Equals("N"))
        foreach (var item in langsDict.Keys)
        {
            while (!answer.Equals("y") && !answer.Equals("n") && !answer.Equals("Y") && !answer.Equals("N"))
            {
                Console.WriteLine("Are you interested in including code files in all languages?");
                answer = Console.ReadLine();
            }
            if (answer.Equals("y") || answer.Equals("Y"))
                languages += (" " + answer);
        }
    string note = "";
    while (!note.Equals("y") && !note.Equals("n") && !note.Equals("Y") && !note.Equals("N"))
    {
        Console.WriteLine("Are you interested in listing the source code as a comment in the bundle file?");
        note = Console.ReadLine();
    }
    string author = "";
    Console.WriteLine("Enter the author name of the file. (If you don't want to enter a name, press enter)");
    author = Console.ReadLine();
    string sort;
    Console.WriteLine("The order of copying the code files, according to the letter of the file name or according to the type of code?");
    Console.WriteLine("enter name/kind, the default is by name");
    sort = Console.ReadLine().Equals("kind") ? "kind" : "name";
    Console.WriteLine("Do you want to delete empty lines? (y/n)");
    string remove = Console.ReadLine();
    while(!remove.Equals("y") && !remove.Equals("n") && !remove.Equals("Y") && !remove.Equals("N"))
    {
        Console.WriteLine("Enter y/n");
        remove = Console.ReadLine();
    }
    var file = new StreamWriter("rspFile.rsp");
    file.Write("bundle ");
    file.Write("-o " + output);
    file.Write(" -l " + languages);
    if (!author.Equals(""))
        file.Write(" -a " + author);
    if (sort.Equals("kind"))
        file.Write(" -s " + sort);
    if (note.Equals("y") || note.Equals("Y"))
        file.Write(" -n ");
    if (remove.Equals("y") || remove.Equals("Y"))
        file.Write(" rel ");
    file.Close();


});

var rootCommand = new RootCommand("root command for bundle CLI");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

rootCommand.InvokeAsync(args);

string[] GetExtentions(string languages)
{
    if (languages.Contains("all"))
        return langsDict.Values.ToArray();
    List<string> ext = new List<string>();
    List<string> langs = languages.Split(' ').ToList();
    langs.ForEach(l => ext.Add(langsDict[l]));
    return ext.ToArray();
}