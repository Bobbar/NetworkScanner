using System;
using System.Threading;


namespace NetworkScanner
{
    class Program
    {
       

        static void Main(string[] args)
        {

            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;


            System.AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            int threads = 0;
            int interval = 0;

            //if (args.Length > 0)
            //{
            //    if (args.Length == 1)
            //    {
            //        bool inttest = int.TryParse(args[0], out threads);
            //        if (inttest)
            //        {
            //            if (threads < 1)
            //            {
            //                Console.WriteLine("Invalid number of scan threads. Value must be greater than 0.");
            //                ShowHelp();
            //                Environment.Exit(-1);
            //            }
            //        }

            //        bool helptest = (args[0].ToUpper() == "-H");
            //        if (helptest)
            //        {
            //            ShowHelp();
            //            Environment.Exit(0);
            //        }

            //    }
            //    else if (args.Length == 2)
            //    {
            //        bool inttest = int.TryParse(args[0], out threads);
            //        if (inttest)
            //        {
            //            if (threads < 1)
            //            {
            //                Console.WriteLine("Invalid number of scan threads. Value must be greater than 0.");
            //                ShowHelp();
            //                Environment.Exit(-1);
            //            }
            //        }

            //        if (args[1].ToUpper() == "-V")
            //        {
            //            Logging.VerboseLog = true;
            //        }
            //        else if (args[1].ToUpper() == "-NL")
            //        {
            //            Logging.LoggingEnabled = false;
            //        }
            //        else
            //        {
            //            Console.WriteLine("Invalid argument '" + args[1] + "'");
            //            ShowHelp();
            //        }
            //    }
            //    else if (args.Length == 3)
            //    {
            //        bool inttest = int.TryParse(args[0], out threads);
            //        if (inttest)
            //        {
            //            if (threads < 1)
            //            {
            //                Console.WriteLine("Invalid number of scan threads. Value must be greater than 0.");
            //                ShowHelp();
            //                Environment.Exit(-1);
            //            }
            //        }

            //        if (args[1].ToUpper() == "-V")
            //        {
            //            Logging.VerboseLog = true;
            //        }
            //        else if (args[1].ToUpper() == "-NL")
            //        {
            //            Logging.LoggingEnabled = false;
            //        }
            //        else
            //        {
            //            Console.WriteLine("Invalid argument '" + args[1] + "'");
            //            ShowHelp();
            //        }


            //        bool intervaltest = int.TryParse(args[2], out interval);
            //        if (!intervaltest)
            //        {
            //            Console.WriteLine("Invalid interval value '" + args[2] + "'");
            //            Environment.Exit(-1);
            //        }

            //    }
            //}

            interval = 2;
            threads = 25;
            Logging.VerboseLog = true;

            // Database.DBFunctions.PopulateGUIDCache();
            //var newScan = new MultiThreadScanner();
            //newScan.StartScan(num);
            Tasker.TaskControl.RunTasks(interval, threads);
            Logging.Verbose("Hitting Console.Read()");
            //Console.Read();
            RunLoop();
            Logging.Verbose("Passed Console.Read()!!?");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logging.Log("Process EXIT!!? ");
        }

        static void RunLoop()
        {
            do
            {
                Logging.Verbose("############### LOOP");
                Thread.Sleep(1000);

            } while (1 == 1);

            Logging.Verbose("############# END LOOP");
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
