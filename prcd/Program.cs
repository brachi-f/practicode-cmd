
using System.CommandLine;


var bundleCommand = new Command("bundle", "bundle code files to a single file");

//options
var languageOption = new Option<string>("language", "A list of file extensions to include in the file");
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
    try
    {
        var bundleFile = File.CreateText(output.FullName);
        string[]? extentions = GetExtentions(languages);

        List<string> allfiles = Directory.GetDirectories(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
        .Where(f => !(f.Contains(@"\obj\") || f.Contains(@"\bin\") || f.Contains(@"\Debug\") || f.Contains(@"\node_modules\"))).ToList();

        List<FileInfo> textFiles = new List<FileInfo>();
        FileInfo info;
        allfiles.ForEach(f =>
        {
            info = new FileInfo(f);
            Directory.GetFiles(f).ToList().ForEach(file =>
            {
                if (extentions == null || extentions.Contains(new FileInfo(file).Extension))
                    textFiles.Add(new FileInfo(file));
            });
        });

        if (sort.Equals("kind"))
            textFiles = textFiles.OrderBy(f => f.Extension).ToList();
        else
            textFiles = textFiles.OrderBy(f => f.Name).ToList();
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
    string name;
    Console.WriteLine("enter name for rsp file");
    name = Console.ReadLine();
    string output;
    Console.WriteLine("enter path and file name: ");
    output = Console.ReadLine();
    string answer = "";
    List<string> extensions = new List<string>();
    while (!answer.Equals("y") && !answer.Equals("n") && !answer.Equals("Y") && !answer.Equals("N"))
    {
        Console.WriteLine("Are you interested in including code files in all languages? (y/n)");
        answer = Console.ReadLine();
    }
    if (answer.Equals("n") || answer.Equals("N"))
    {
        Console.WriteLine("enter list of extensions you want to include in the bundle file. to end press END");
        while (!answer.Equals("END"))
        {
            answer = Console.ReadLine();
            if (answer[0] == '.')
                extensions.Add(answer);
            else Console.WriteLine(answer + " is invalid extension");
        }
    }

    string note = "";
    while (!note.Equals("y") && !note.Equals("n") && !note.Equals("Y") && !note.Equals("N"))
    {
        Console.WriteLine("Are you interested in listing the source code as a comment in the bundle file? (y/n)");
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
    while (!remove.Equals("y") && !remove.Equals("n") && !remove.Equals("Y") && !remove.Equals("N"))
    {
        Console.WriteLine("Enter y/n");
        remove = Console.ReadLine();
    }
    var file = new StreamWriter($"{name}.rsp");
    file.Write("bundle ");
    file.Write("-o " + output);
    string languages = extensions.Count() == 0 ? "all" : splitExt(extensions);
    file.Write(" -l " + '"' + languages + '"');
    if (!author.Equals(""))
        file.Write(" -a " + author);
    if (sort.Equals("kind"))
        file.Write(" -s " + sort);
    if (note.Equals("y") || note.Equals("Y"))
        file.Write(" -n");
    if (remove.Equals("y") || remove.Equals("Y"))
        file.Write(" -rel ");
    file.Close();


});

string splitExt(List<string> extensions)
{
    string split = "";
    extensions.ForEach(e => split += (' ' + e));
    split = split.Substring(1);
    return split;
}
string[]? GetExtentions(string extensions)
{
    if (extensions.Contains("all"))
        return null;
    string[] extension = extensions.Split(' ');
    foreach (var ex in extension)
        if (ex is not null)
            if (ex[0] != '.')
                Console.WriteLine(ex + " is not valid extension");
    return extension.Where(e => e[0] == '.').ToArray();
}
var rootCommand = new RootCommand("root command for bundle CLI");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

rootCommand.InvokeAsync(args);

