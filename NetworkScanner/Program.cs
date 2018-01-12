using System;
using NetworkScanner.NetworkScanning;

namespace NetworkScanner
{
    class Program
    {


        static void Main(string[] args)
        {

            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            System.AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            if (args.Length == 1)
            {

                var logArg = args[0].ToUpper();

                if (logArg == "-V") //Verbose logging
                {
                    Logging.VerboseLog = true;
                    Console.WriteLine("Log args: Verbose logs.");
                }
                else if (logArg == "-D") //Debug logging
                {
                    Logging.VerboseLog = true;
                    Logging.DebugLog = true;
                    Console.WriteLine("Log args: Debug logs.");
                }
                else if (logArg == "-NL") //No Logging.
                {
                    Logging.LoggingEnabled = false;
                    Console.WriteLine("Log args: Log disabled.");
                }
                else if (logArg == "-H")
                {
                    ShowHelp();
                }
            }

            Logging.Debug("Launching scan...");
            SingleThreadScanner.StartScan();
            Logging.Debug("Passed scan call...");
            Environment.Exit(0);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logging.Debug("Process Exiting.");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: [thisdll].dll [log args]");
            Console.WriteLine("Valid arguments:");
            Console.WriteLine("-v = Verbose output. (Default: false)");
            Console.WriteLine("-d = Debug logging. (Default: false)");
            Console.WriteLine("-nl = Disable all logging. (Default: false)");

        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

    }
}
