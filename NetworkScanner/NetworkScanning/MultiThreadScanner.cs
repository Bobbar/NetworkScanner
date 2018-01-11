using NetworkScanner.Database;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NetworkScanner.NetworkScanning
{
    public static class MultiThreadScanner
    {
        public static bool ScanRunning = false;
        private static List<ScanResult> pingResults = new List<ScanResult>();
        private static List<string> scanList;
        private const int defaultScanThreads = 10;
        private static int maxThreads = defaultScanThreads;
        private static int currentThreads = 0;
        private static int currentScanIdx = 0;
        private static object thislock = new object();
        private static long startTime;


        public static void StartScan(int scanThreads = defaultScanThreads)
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

            pingResults = new List<ScanResult>();
            scanList = new List<string>();

            startTime = DateTime.Now.Ticks;
            scanList = DBFunctions.Hostnames();
            Logging.Info(scanList.Count.ToString() + " hostnames will be scanned.");

            ScanLoop();

        }

        private static void ScanLoop()
        {
            currentThreads = 0;
            currentScanIdx = 0;


            do
            {
                if (currentThreads < maxThreads && currentScanIdx < scanList.Count)
                {
                    ScanRunning = true;
                    StartPingThread(scanList[currentScanIdx]);
                    currentScanIdx++;
                }
                Thread.Sleep(100);
                Logging.Verbose(currentThreads.ToString());
            } while (!ScanComplete());

        }

        private static void StartPingThread(string hostname)
        {

            Logging.Verbose(currentScanIdx + " of " + (scanList.Count - 1) + ": " + hostname);
            Pinger pinger = new Pinger(hostname);
            Thread t = new Thread(pinger.StartPing);
            pinger.PingComplete += PingComplete;
            t.Start();
            currentThreads++;

        }

        private static void PingComplete(object sender, EventArgs e)
        {

            try
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

                    Logging.Log("Scan Complete.  Num of results: " + pingResults.Count.ToString());
                    Logging.Log("Inserting changes into database...");

                    if (DBFunctions.InsertScanResults(pingResults))
                    {
                        Logging.Log("  Success!");
                        DBFunctions.ReportRun(true);
                    }
                    else
                    {
                        Logging.Log("  Failed!");
                        DBFunctions.ReportRun(false);
                    }
                    var elapTime = (((DateTime.Now.Ticks - startTime) / 10000) / 1000);
                    Logging.Log("Runtime: " + elapTime + " s");
                    Logging.Log("");
                    Logging.Log("");
                    Logging.Log("");

                    pingResults.Clear();
                    pingResults = null;
                    scanList.Clear();
                    scanList = null;

                    ScanRunning = false;
                    //Environment.Exit(0);
                }

            }
            catch (Exception ex)
            {
                ScanRunning = false;
                Logging.Error(ex.Message);
                DBFunctions.ReportRun(false);
            }

        }

        private static bool ScanComplete()
        {
            if (currentScanIdx >= (scanList.Count) && currentThreads == 0)
            {
                return true;
            }
            return false;
        }

    }
}
