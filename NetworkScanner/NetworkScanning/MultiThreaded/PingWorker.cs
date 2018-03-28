using System;
using System.Collections.Generic;
using System.Text;
using NetworkScanner.Database;
using System.Net.NetworkInformation;

namespace NetworkScanner.NetworkScanning
{
    public class PingWorker
    {
        private List<ScanResult> pingResults = new List<ScanResult>();
        private List<string> hostsList;
        private Ping pinger;
        private const int pingTimeOut = 500;
        private int workerID;

        public PingWorker(List<string> hosts, int id)
        {
            workerID = id;
            hostsList = hosts;
            pinger = new Ping();
        }

        public List<ScanResult> GetResults()
        {
            Logging.Verbose("[" + workerID + "] Hostcount: " + hostsList.Count);
            foreach (var host in hostsList)
            {
                var pingReply = PingHost(host);

                // Errors during ping method returns a null, check for that first.
                if (pingReply != null)
                {
                    var pingResult = host + " - " + pingReply.Address.ToString() + " - " + pingReply.Status.ToString();
                    Logging.Verbose("[" + workerID + "] Result: " + pingResult);

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
            Logging.Verbose("[" + workerID + "]  ##### DONE. Results: " + pingResults.Count);
            return pingResults;
        }

        /// <summary>
        /// Pings a hostname and returns a PingReply. Any errors return null.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        private PingReply PingHost(string hostname)
        {
            try
            {
                var pingOptions = new PingOptions();
                pingOptions.DontFragment = true;
                byte[] pingBuffer = Encoding.ASCII.GetBytes("ping");
                return pinger.Send(hostname, pingTimeOut, pingBuffer, pingOptions);
            }
            catch (Exception ex)
            {
                Logging.Warning("[" + workerID + "] " + hostname + " - " + ex.Message);
                return null;
            }
        }

    }
}
