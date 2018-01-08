using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NetworkScanner.Database;

namespace NetworkScanner.NetworkScanning
{
    public class MultiThreadScanner
    {
        private System.Timers.Timer threadTimer;
        private List<ScanResult> pingResults = new List<ScanResult>();
        private List<string> scanList;
        private const int defaultScanThreads = 40;
        private int maxThreads = defaultScanThreads;
        private int currentThreads = 0;
        private int currentHostIdx = 0;
        private object thislock = new object();
        private long startTime;

        public MultiThreadScanner(int scanThreads = defaultScanThreads)
        {
            if (scanThreads < 1)
            {
                maxThreads = scanThreads;
            }
            else
            {
                maxThreads = defaultScanThreads;
            }

            InitTimer();
        }

        public void StartScan()
        {
            startTime = DateTime.Now.Ticks;
            scanList = DBFunctions.Hostnames();
            threadTimer.Start();
        }

        private void InitTimer()
        {
            threadTimer = new System.Timers.Timer();
            threadTimer.Interval = 100;
            threadTimer.Enabled = true;
            threadTimer.Elapsed += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (currentThreads < maxThreads && currentHostIdx < scanList.Count)
            {
                StartPingThread(scanList[currentHostIdx]);
                currentHostIdx++;
            }
        }

        private void StartPingThread(string ip)
        {

            Console.WriteLine(currentHostIdx + " of " + (scanList.Count - 1) + "  Start ping: " + ip);

            Pinger pinger = new Pinger(ip);
            Thread t = new Thread(pinger.StartPing);
            pinger.PingComplete += PingComplete;
            t.Start();

            currentThreads++;
        }

        private void PingComplete(object sender, EventArgs e)
        {
            var pingEvent = (Pinger.PingerCompleteEventArgs)e;

            if (pingEvent.Success)
            {
                var pingResult = pingEvent.Hostname + " - " + pingEvent.PingIP + " - " + pingEvent.Success.ToString();
                var deviceGUID = DBFunctions.DeviceGUID(pingEvent.Hostname);

                if (!string.IsNullOrEmpty(deviceGUID))
                {
                    pingResults.Add(new ScanResult(deviceGUID, pingEvent.PingIP, pingEvent.Success, pingEvent.Hostname));
                }

                Console.WriteLine(pingResult);
            }

            lock (thislock)
            {
                currentThreads--;
            }

            if (ScanComplete())
            {
                threadTimer.Stop();
                Console.WriteLine("Scan Complete.  Num of results: " + pingResults.Count.ToString());
                Console.WriteLine("Inserting changes into database...");

                if (DBFunctions.InsertScanResults(pingResults))
                {
                    Console.WriteLine("  Success!");
                }
                else
                {
                    Console.WriteLine("  Failed!");
                }
                var elapTime = (((DateTime.Now.Ticks - startTime) / 10000) / 1000);
                Console.WriteLine("Runtime: " + elapTime + " s");
                Environment.Exit(0);

            }

        }

        private bool ScanComplete()
        {
            if (currentHostIdx >= (scanList.Count) && currentThreads == 0)
            {
                return true;
            }
            return false;
        }


    }
}
