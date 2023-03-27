namespace PdbExtractor
{
    internal class Printer
    {
        private static void printHelp()
        {
            var Help = @"PDB extractor
###########
Prints GUID and path of the related PDB file
Usage:

PDB-extractor.exe [OPTIONS] TARGETS

[OPTIONS]:
    -a  Print all possible information, including PDB file parsed info. 

TARGETS - paths to the PE files
";
            Console.WriteLine(Help);
        }

        private static void parseFilePaths(string[] args, List<string> files, int printAll)
        {
            for (int i = printAll; i < args.Length; i++)
            {
                var arg = args[i];
                try
                {
                    var searchPath = Path.GetDirectoryName(Path.GetFullPath(arg));
                    var searchPattern = Path.GetFileName(arg);
                    files.AddRange(Directory.GetFiles(searchPath, searchPattern, SearchOption.TopDirectoryOnly));
                }
                catch
                {
                    Console.Error.WriteLine(String.Format("Error occured, while parsing \"{0}\" file path(s)", arg));
                    Console.WriteLine();
                }
            }
        }

        private static void printDirectoryInfo(DebugDirectory dir, int printAll)
        {
            try
            {
                Console.WriteLine(dir.ToString());
                if (printAll == 1)
                {
                    var fileStreamOfPdb = new FileStream(dir.getPath(), FileMode.Open, FileAccess.Read);
                    var pdbParser = new PdbParser(fileStreamOfPdb);
                    Console.WriteLine("Pdb file information:");
                    pdbParser.printInfo();
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine();
            }
        }

        public static void printInfo(string[] args)
        {
            int printAll = 0;
            if (args.Length == 0)
            {
                Printer.printHelp();
                return;
            }
            if (args[0] == "-a")
            {
                printAll++;
            }
            List<String> files = new List<string>();
            parseFilePaths(args, files, printAll);
            foreach (var file in files)
            {
                Console.WriteLine(String.Format("PE File: {0}", file));
                try
                {
                    var fileStreamOfExecutable = new FileStream(file, FileMode.Open, FileAccess.Read);
                    var extractor = new PdbExtractor.PathExtractor(fileStreamOfExecutable);
                    var directories = extractor.getDirectories();
                    Console.WriteLine("#Pdb(s)#");
                    Console.WriteLine();
                    Array.ForEach(directories, dir => printDirectoryInfo(dir, printAll));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.WriteLine();
                }
            }
        }
    }
}