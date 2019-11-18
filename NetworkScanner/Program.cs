using System;
using NetworkScanner.NetworkScanning;
using CommandLine;

namespace NetworkScanner
{
    class Program
    {
        private static int _pingThreads = 1;
        private static int _maxEntries = -1;

        public class Options
        {
            [Option('t', "threads", Default = 1, HelpText = "Scan threads to use.")]
            public int Threads { get; set; }

            [Option('m', "maxentries", Default = -1, HelpText = "Max historical entries allowed. (Default = Allow all)")]
            public int MaxEntries { get; set; }

            [Option('l', "loglevel", Default = 1, HelpText = "Logging level. (0=None, 1=Default, 2=Verbose, 3=Debug)")]
            public int LogLevel { get; set; }
        }

        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            System.AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Parser.Default.ParseArguments<Options>(args).WithParsed(opts => Run(opts));
        }

        static void Run(Options options)
        {
            // Process options.
            ParseThreadArg(options.Threads);
            ParseLogArg(options.LogLevel);
            if (options.MaxEntries > 0)
                _maxEntries = options.MaxEntries;

            Logging.Debug("Launching scan...");
            MultiThreadScanner.StartScan(_pingThreads, _maxEntries);
            Logging.Debug("Passed scan call...");

            Environment.Exit(0);
        }

        private static void ParseThreadArg(int threads)
        {
            if (threads > 0 && threads < 20)
                _pingThreads = threads;
            else
                _pingThreads = 1;
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
