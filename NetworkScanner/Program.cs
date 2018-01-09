using System;
using NetworkScanner.NetworkScanning;

namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {

            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            int num = 0;
            if (args.Length > 0)
            {
                if (args.Length == 1)
                {
                    bool inttest = int.TryParse(args[0], out num);
                    if (inttest)
                    {
                        if (num < 1)
                        {
                            Console.WriteLine("Invalid number of scan threads. Value must be greater than 0.");
                            ShowHelp();
                            Environment.Exit(-1);
                        }
                    }

                    bool helptest = (args[0].ToUpper() == "-H");
                    if (helptest)
                    {
                        ShowHelp();
                        Environment.Exit(0);
                    }

                }
                else if (args.Length == 2)
                {
                    bool inttest = int.TryParse(args[0], out num);
                    if (inttest)
                    {
                        if (num < 1)
                        {
                            Console.WriteLine("Invalid number of scan threads. Value must be greater than 0.");
                            ShowHelp();
                            Environment.Exit(-1);
                        }
                    }

                    bool verbtest = (args[1].ToUpper() == "-V");
                    if (verbtest)
                    {
                        Logging.VerboseLog = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid argument '" + args[1] + "'");
                        ShowHelp();
                    }
                }
            }



            Database.DBFunctions.PopulateGUIDCache();
            var newScan = new MultiThreadScanner();
            newScan.StartScan(num);
            Console.Read();
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: [thisdll].dll [arg1] [arg2]");
            Console.WriteLine("Valid arguments: [#] [-v]");
            Console.WriteLine("# = Number of scan threads. (Default: 10)");
            Console.WriteLine("-v = Verbose output. (Default: false)");
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

    }
}
