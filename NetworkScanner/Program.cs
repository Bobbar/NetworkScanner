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

                if (args[0].ToUpper() == "-V")
                {
                    Logging.VerboseLog = true;
                }

            }

            Logging.Verbose("Launching scan...");
            SingleThreadScanner.StartScan();
            Logging.Verbose("Passed scan call...");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logging.Log("Process Exiting.");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: [thisdll].dll [arg1] [arg2]");
            Console.WriteLine("Valid arguments: [#] [logging]");
            Console.WriteLine("# = Number of scan threads. (Default: 10)");
            Console.WriteLine("-v = Verbose output. (Default: false)");
            Console.WriteLine("-nl = Disable logging. (Default: false)");
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

    }
}
