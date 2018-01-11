using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NetworkScanner.NetworkScanning;

namespace NetworkScanner.Tasker
{


    public static class TaskControl
    {

        private static System.Threading.Timer taskTimer;
        private static int selectedInterval;
        private static int selectedThreads;

        public static void RunTasks(int interval, int threads)
        {

            if (interval > 0)
            {
                selectedInterval = interval;
                selectedThreads = threads;
                InitTaskTimer(interval);
                
            }

        }

        private static void InitTaskTimer(int intervalMins)
        {

            TimeSpan interval = new TimeSpan(0, intervalMins, 0);
            taskTimer = new System.Threading.Timer(new TimerCallback(TimerTick), null, new TimeSpan(0), interval);
        }

        private static void TimerTick(object state)
        {
            // Start a new scan.
            Logging.Log("Tasker is starting new scan...");
            if (!MultiThreadScanner.ScanRunning)
            {
                Logging.Verbose("Getting list of hostnames...");
                Database.DBFunctions.PopulateGUIDCache();
                Logging.Verbose("Got list of hostnames.");
                MultiThreadScanner.StartScan(selectedThreads);
            }
            else
            {
                Logging.Error("Could not start new scan, a scan is already in progress.");
            }

        }


    }
}
