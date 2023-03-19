﻿using PdbExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

PDB-extractor.exe [OPTIONS] [TARGETS]

OPTIONS:
    -a  Print all possible information, including PDB file parsed info. 

[TARGETS] - full paths to the PE files/group of files
";
            Console.WriteLine(Help);
        }


        private static List<string> parseFilePaths(string[] args, int printAll)
        {
            List<string> files = new List<string>();
            var rootPath = Path.GetPathRoot(Directory.GetCurrentDirectory());

            for (int i = printAll; i < args.Length; i++)
            {
                var arg = args[i];
                try
                {
                    var searchPattern = arg;
                    var searchPath = rootPath;
                    var fileRootPath = rootPath;
                    var indexOfFirstAsterisk = arg.IndexOf('*');
                    if (indexOfFirstAsterisk == -1)
                    {
                        fileRootPath = Path.GetPathRoot(arg);
                    }
                    else
                    {
                        fileRootPath = Path.GetPathRoot(arg.Substring(0, indexOfFirstAsterisk));
                    }

                    if (fileRootPath == rootPath)
                    {
                        searchPattern = arg.Substring(searchPath.Length);
                    }
                    else if (fileRootPath == string.Empty || fileRootPath == null)
                    {
                        searchPath = Directory.GetCurrentDirectory();
                    }
                    else
                    {
                        searchPath = fileRootPath;
                        searchPattern = arg.Substring(searchPath.Length);
                    }

                    files.AddRange(Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories));
                }
                catch
                {
                    Console.Error.WriteLine(String.Format("Error occured, while parsing \"{0}\" file path(s)", arg));
                    Console.WriteLine();
                }
            }
            return files;
        }

        public static void printPdbInfo(string[] args)
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
            List<string> files = parseFilePaths(args, printAll);
            for (int i = 0; i < files.Count; i++)
            {
                Console.WriteLine(String.Format("PE File: {0}", files[i]));
                try
                {
                    var bytesOfExecutable = File.ReadAllBytes(files[i]);
                    var extractor = new PdbExtractor.PathExtractor(bytesOfExecutable);
                    var path = extractor.getPath();
                    var guid = extractor.getGuid();
                    Console.WriteLine(String.Format("Pdb file path: {0}", path));
                    Console.WriteLine(String.Format("GUID in PE: {0}", guid));
                    Console.WriteLine();
                    if (printAll == 1)
                    {
                        var bytesOfPdb = File.ReadAllBytes(path);
                        var pdbParser = new PdbParser(bytesOfPdb);
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
        }
    }
}