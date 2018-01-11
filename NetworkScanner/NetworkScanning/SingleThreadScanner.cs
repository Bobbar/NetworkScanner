using System;
using System.Collections.Generic;
using System.Text;
using NetworkScanner.Database;
using System.Net.NetworkInformation;

namespace NetworkScanner.NetworkScanning
{


    public static class SingleThreadScanner
    {

        private static List<ScanResult> pingResults = new List<ScanResult>();
        private static List<string> scanList;
        private static Ping myPing;
        private const int pingTimeOut = 500;
        private static long startTime;


        public static void StartScan()
        {
            myPing = new Ping();
            startTime = DateTime.Now.Ticks;

            Logging.Log("");
            Logging.Log("");
            Logging.Log("");
            Logging.Log("Starting Network Scan.");
            Logging.Log("Populating GUID Cache...");
            DBFunctions.PopulateGUIDCache();
            Logging.Log("Done.");

            Logging.Log("Getting Hostnames...");
            scanList = DBFunctions.Hostnames();
            Logging.Log("Done.");

            Logging.Info(scanList.Count.ToString() + " hostnames will be scanned.");

            foreach (var host in scanList)
            {
                Logging.Log(scanList.IndexOf(host) + " of " + (scanList.Count - 1) + ": " + host);

                var pingReply = PingHost(host);

                if (pingReply != null)
                {
                    var pingResult = host + " - " + pingReply.Address.ToString() + " - " + pingReply.Status.ToString();
                    Logging.Log("Result: " + pingResult);

                    if (pingReply.Status == IPStatus.Success)
                    {

                        var deviceGUID = DBFunctions.DeviceGUID(host);
                        if (!string.IsNullOrEmpty(deviceGUID))
                        {
                            pingResults.Add(new ScanResult(deviceGUID, pingReply.Address.ToString(), true, host));
                        }

                    }
                }
            }

            Logging.Log("Scan Complete.  Num of results: " + pingResults.Count.ToString());
            Logging.Log("Inserting changes into database...");

            if (DBFunctions.InsertScanResults(pingResults))
            {
                Logging.Log("Done!");
                DBFunctions.ReportRun(true);
            }
            else
            {
                Logging.Log("Failed!");
                DBFunctions.ReportRun(false);
            }
            var elapTime = (((DateTime.Now.Ticks - startTime) / 10000) / 1000);
            Logging.Log("Runtime: " + elapTime + " s");
            Environment.Exit(0);

        }

        private static PingReply PingHost(string hostname)
        {
            try
            {
                var pingOptions = new PingOptions();
                pingOptions.DontFragment = true;
                byte[] pingBuffer = Encoding.ASCII.GetBytes("ping");
                return myPing.Send(hostname, pingTimeOut, pingBuffer, pingOptions);
            }
            catch (Exception ex)
            {
                Logging.Warning(hostname + " - " + ex.Message);
                return null;
            }
        }


    }
}
