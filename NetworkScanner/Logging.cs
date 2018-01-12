using System;

namespace NetworkScanner
{
    public static class Logging
    {
        public static bool VerboseLog { get; set; } = false;
        public static bool LoggingEnabled { get; set; } = true;
        public static bool DebugLog { get; set; } = false;

        public static void Log(string message)
        {
           if (LoggingEnabled) Console.WriteLine(DateTime.Now.ToString() + ": " + message);
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

        public static void Debug(string message)
        {
            if (DebugLog) Log(message);
        }
    }
}
