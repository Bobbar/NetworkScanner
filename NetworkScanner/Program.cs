using System;
using NetworkScanner.NetworkScanning;

namespace NetworkScanner
{
    class Program
    {

        private static int pingThreads = 1;

        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            System.AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    var upArg = arg.ToUpper();
                    int threads;
                    bool isNum = int.TryParse(upArg, out threads);

                    if (isNum)
                    {
                        ParseThreadArg(threads);
                    }
                    else
                    {
                        ParseLogArg(upArg);
                    }
                }
            }

            Logging.Debug("Launching scan...");
            MultiThreadScanner.StartScan(pingThreads);
            Logging.Debug("Passed scan call...");

            Environment.Exit(0);
        }

        private static void ParseThreadArg(int threads)
        {
            if (threads < 1 || threads > 20)
            {
                pingThreads = 1;
            }
            else
            {
                pingThreads = threads;
            }
        }

        private static void ParseLogArg(string arg)
        {
            if (arg == "-V") //Verbose logging
            {
                Logging.VerboseLog = true;
                Console.WriteLine("Log args: Verbose logs.");
            }
            else if (arg == "-D") //Debug logging
            {
                Logging.VerboseLog = true;
                Logging.DebugLog = true;
                Console.WriteLine("Log args: Debug logs.");
            }
            else if (arg == "-NL") //No Logging.
            {
                Logging.LoggingEnabled = false;
                Console.WriteLine("Log args: Log disabled.");
            }
            else if (arg == "-H")
            {
                ShowHelp();
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logging.Debug("Process Exiting.");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: [thisdll].dll [log args] [# threads]");
            Console.WriteLine("Example (Verbose with 4 threads): [thisdll].dll -v 4");
            Console.WriteLine("Valid arguments:");
            Console.WriteLine("-v = Verbose output. (Default: false)");
            Console.WriteLine("-d = Debug logging. (Default: false)");
            Console.WriteLine("-nl = Disable all logging. (Default: false)");
            Console.WriteLine("threads = Number of scan threads to use. (Default: 1 Max: 20)");

        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

    }
}
