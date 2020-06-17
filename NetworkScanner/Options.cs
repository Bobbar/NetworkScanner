using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace NetworkScanner
{
    public class Options
    {
        private int _threads = 1;
        private int _maxEntries = -1;
        private int _logLevel = 1;
        private int _timeOut = 500;

        [Option('t', "threads", Default = 1, HelpText = "Scan threads to use.")]
        public int Threads 
        {
            get { return _threads; }

            set 
            {
                if (value > 0 && value <= 20)
                    _threads = value;
                else
                    _threads = 1;
            }
        }

        [Option('m', "maxentries", Default = -1, HelpText = "Max historical entries allowed. (Default = Allow all)")]
        public int MaxEntries
        {
            get { return _maxEntries; }
            
            set
            {
                if (value >= -1)
                    _maxEntries = value;
            }
        }

        [Option('l', "loglevel", Default = 1, HelpText = "Logging level. (0=None, 1=Default, 2=Verbose, 3=Debug)")]
        public int LogLevel 
        { 
            get { return _logLevel; }

            set
            {
                if (value >= 0 && value <= 3)
                    _logLevel = value;
            }
        }

        [Option('o', "timeout", Default = 500, HelpText = "Ping timeout in milliseconds. Will effect total runtime. (Default = 500)")]
        public int TimeOut 
        {
            get { return _timeOut; }

            set
            {
                if (value > 10 && value <= 4000)
                    _timeOut = value;
            }
        }

        public string ToString()
        {
            return $"Threads: {Threads}  MaxEntries: {MaxEntries}  LogLevel: {LogLevel}  TimeOut: {TimeOut}";
        }
    }
}
