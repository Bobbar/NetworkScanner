using NetworkScanner.Database;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NetworkScanner.NetworkScanning
{
    public class MultiThreadScanner
    {
        private System.Timers.Timer threadTimer;
        private List<ScanResult> pingResults = new List<ScanResult>();
        private List<string> scanList;
        private const int defaultScanThreads = 10;
        private int maxThreads = defaultScanThreads;
        private int currentThreads = 0;
        private int currentScanIdx = 0;
        private object thislock = new object();
        private long startTime;

        public MultiThreadScanner()
        {
            InitTimer();
        }

        public void StartScan(int scanThreads = defaultScanThreads)
        {
            if (scanThreads > 1)
            {
                maxThreads = scanThreads;
            }
            else
            {
                maxThreads = defaultScanThreads;
            }

            Logging.Log("Starting new scan using " + maxThreads + " threads.");

            startTime = DateTime.Now.Ticks;
            scanList = DBFunctions.Hostnames();
            Logging.Info(scanList.Count.ToString() + " hostnames will be scanned.");
            threadTimer.Start();
        }

        private void InitTimer()
        {
            threadTimer = new System.Timers.Timer();
            threadTimer.Interval = 50;
            threadTimer.Enabled = true;
            threadTimer.Elapsed += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (currentThreads < maxThreads && currentScanIdx < scanList.Count)
            {
                StartPingThread(scanList[currentScanIdx]);
                currentScanIdx++;
            }
        }

        private void StartPingThread(string hostname)
        {

            Logging.Verbose(currentScanIdx + " of " + (scanList.Count - 1) + ": " + hostname);
            Pinger pinger = new Pinger(hostname);
            Thread t = new Thread(pinger.StartPing);
            pinger.PingComplete += PingComplete;
            t.Start();

            currentThreads++;
        }

        private void PingComplete(object sender, EventArgs e)
        {
            var pingEvent = (Pinger.PingerCompleteEventArgs)e;

            var pingResult = pingEvent.Hostname + " - " + pingEvent.PingIP + " - " + pingEvent.Success.ToString();
            Logging.Verbose("Result: " + pingResult);

            if (pingEvent.Success)
            {
                var deviceGUID = DBFunctions.DeviceGUID(pingEvent.Hostname);
                if (!string.IsNullOrEmpty(deviceGUID))
                {
                    pingResults.Add(new ScanResult(deviceGUID, pingEvent.PingIP, pingEvent.Success, pingEvent.Hostname));
                }

            }

            lock (thislock)
            {
                currentThreads--;
            }

            if (ScanComplete())
            {
                threadTimer.Stop();
                Logging.Log("Scan Complete.  Num of results: " + pingResults.Count.ToString());
                Logging.Log("Inserting changes into database...");

                if (DBFunctions.InsertScanResults(pingResults))
                {
                    Logging.Log("  Success!");
                }
                else
                {
                    Logging.Log("  Failed!");
                }
                var elapTime = (((DateTime.Now.Ticks - startTime) / 10000) / 1000);
                Logging.Log("Runtime: " + elapTime + " s");
                Console.WriteLine();
                Console.WriteLine();
                Environment.Exit(0);
            }

        }

        private bool ScanComplete()
        {
            if (currentScanIdx >= (scanList.Count) && currentThreads == 0)
            {
                return true;
            }
            return false;
        }

    }
}
