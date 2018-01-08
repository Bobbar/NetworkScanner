using System;
using NetworkScanner.NetworkScanning;

namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            int num = 0;
            if (args.Length > 0)
            {
                bool inttest = int.TryParse(args[0], out num);
                if (inttest)
                {
                    if (num < 1)
                    {
                        Console.WriteLine("Invalid number of scan threads. Value must be greater than 0.");
                        Environment.Exit(-1);
                    }
                }
            }

            Database.DBFunctions.PopulateGUIDCache();
            var newScan = new MultiThreadScanner();
            newScan.StartScan();
           Console.ReadKey();
        }
    }
}
