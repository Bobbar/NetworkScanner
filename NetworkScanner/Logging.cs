using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkScanner
{
    public static class Logging
    {
        public static bool VerboseLog { get; set; } = false;

        public static void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + ": " + message);
        }

        public static void Error(string message)
        {
            Log("ERROR: " + message);
        }

        public static void Warning(string message)
        {
            if (VerboseLog) Log("WARNING: " + message);
        }

        public static void Info(string message)
        {
            Log("INFO: " + message);
        }

        public static void Verbose(string message)
        {
            if (VerboseLog) Log(message);
        }
    }
}
