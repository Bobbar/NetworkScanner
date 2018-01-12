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
        private static List<string> hostsList;
        private static Ping myPing;
        private const int pingTimeOut = 500;
        private static long startTime;

        /// <summary>
        /// Start a new network scan.
        /// </summary>
        public static void StartScan()
        {
            myPing = new Ping();
            startTime = DateTime.Now.Ticks;

            Console.WriteLine("\n \n");
            Logging.Log("Starting Network Scan.");
          
            // Populate the GUID cache dictionary.
            DBFunctions.PopulateGUIDCache();
           
            Logging.Log("Getting Hostnames...");
            // Populate the list of hostnames to be pinged.
            hostsList = DBFunctions.Hostnames();
            Logging.Log("Done.");

            Logging.Info(hostsList.Count.ToString() + " hostnames will be scanned.");
            // Iterate through the list of hostnames.
            foreach (var host in hostsList)
            {
                Logging.Verbose((hostsList.IndexOf(host) + 1) + " of " + hostsList.Count + ": " + host);

                // Get the ping reply.
                var pingReply = PingHost(host);

                // Errors during ping method returns a null, check for that first.
                if (pingReply != null)
                {

                    var pingResult = host + " - " + pingReply.Address.ToString() + " - " + pingReply.Status.ToString();
                    Logging.Verbose("Result: " + pingResult);

                    // Add only successful pings to the reply list.
                    if (pingReply.Status == IPStatus.Success)
                    {
                        // Get the device GUID from the hostname.
                        var deviceGUID = DBFunctions.DeviceGUID(host);

                        // Make sure a GUID was found.
                        if (!string.IsNullOrEmpty(deviceGUID))
                        {
                            pingResults.Add(new ScanResult(deviceGUID, pingReply.Address.ToString(), true, host));
                        }

                    }
                }
            }

            Logging.Log("Scan Complete.  Num of results: " + pingResults.Count.ToString());
            Logging.Log("Inserting changes into database...");

            // Add all the successful results to the DB.
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
           

        }

        /// <summary>
        /// Pings a hostname and returns a PingReply. Any errors return null.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
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
