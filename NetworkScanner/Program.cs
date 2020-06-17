using System;
using NetworkScanner.NetworkScanning;
using CommandLine;

namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            System.AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Parser.Default.ParseArguments<Options>(args).WithParsed(opts => Run(opts));
        }

        static void Run(Options options)
        {
            // Process options.
            ParseLogArg(options.LogLevel);

            Logging.Debug("Launching scan...");
            MultiThreadScanner.StartScan(options);
            Logging.Debug("Passed scan call...");

            Environment.Exit(0);
        }

        private static void ParseLogArg(int level)
        {
            switch (level)
            {
                case 0:
                    Logging.LoggingEnabled = false;
                    break;

                case 1:
                    Logging.LoggingEnabled = true;
                    break;

                case 2:
                    Logging.VerboseLog = true;
                    Console.WriteLine("Verbose logs enabled.");
                    break;

                case 3:
                    Logging.VerboseLog = true;
                    Logging.DebugLog = true;
                    Console.WriteLine("Debug logs enabled.");
                    break;
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logging.Debug("Process Exiting.");
        }
     
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }
    }
}
