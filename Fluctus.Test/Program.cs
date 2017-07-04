using System;
using System.IO;
using System.Linq;

namespace Fluctus.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("Syntax: <options (-c, -e, -l)> <archive> <password> <file_1> [file_2] [file_...]");
                return;
            }

            if (args[0] == "-a")
            {
                if (args.Length < 4)
                {
                    Console.WriteLine("<archive> <password> <file_1> [file_2] [file_...]");
                    return;
                }

                var fileList = args.ToList();
                fileList.RemoveAt(0); //  options
                fileList.RemoveAt(0); //  archive
                fileList.RemoveAt(0); //  password

                var container = new Container(args[1], args[2]);
                foreach (var entry in fileList)
                {
                    if (!File.Exists(entry) && !Directory.Exists(entry))
                    {
                        Console.WriteLine($"'{entry}' does not exist. Skipping...");
                        continue;
                    }

                    if (!File.GetAttributes(entry).HasFlag(FileAttributes.Directory))
                    {
                        AddFile(container, entry);
                    }
                    else
                    {
                        Console.WriteLine($"Adding directory '{entry}'...");
                        var files = Directory.GetFiles(entry);
                        foreach (var file in files)
                        {
                            AddFile(container, file);
                        }
                    }
                }
                container.Write();
                return;
            }

            if (args[0] == "-e")
            {
                if (args.Length < 3 || args.Length > 4)
                {
                    Console.WriteLine("Syntax: <archive> <password>");
                    return;
                }

                if (!File.Exists(args[1]))
                {
                    Console.WriteLine("Archive does not exist.");
                    return;
                }

                var container = Container.Read(args[1], args[2]);
                if (container.Header.MagicString != "FLUC")
                {
                    Console.WriteLine("File is not a Fluctus archive.");
                    return;
                }

                if (args.Length == 3)
                {
                    try
                    {
                        Console.Write("Now extracting the archive... ");
                        container.ExtractAll();
                        Console.WriteLine("PASS.");
                    }
                    catch
                    {
                        Console.WriteLine("FAIL.");
                        return;
                    }
                }

                if (args.Length == 4)
                {
                    if (!container.FileExists(args[3]))
                    {
                        Console.WriteLine("File does not exist in the specified container.");
                        return;
                    }

                    try
                    {
                        Console.Write("Now extracting the file... ");
                        container.ExtractFile(args[3], args[3]);
                        Console.WriteLine("PASS.");
                    }
                    catch
                    {
                        Console.WriteLine("FAIL.");
                    }
                }
            }

            if (args[0] == "-l")
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Syntax: <archive>");
                    return;
                }

                if (!File.Exists(args[1]))
                {
                    Console.WriteLine("Archive does not exist.");
                    return;
                }

                var container = Container.Read(args[1], "");
                if (container.Header.MagicString != "FLUC")
                {
                    Console.WriteLine("File is not a Fluctus archive.");
                    return;
                }

                foreach (var fileName in container.GetFiles())
                {
                    Console.WriteLine(fileName);
                }
            }
        }

        private static void AddFile(Container container, string entry)
        {
            Console.Write($"Adding {entry}... ");
            try
            {
                container.AddFile(entry);
                Console.WriteLine("PASS.");
            }
            catch
            {
                Console.WriteLine("FAIL.");
            }
        }
    }
}
