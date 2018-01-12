namespace NetworkScanner.NetworkScanning
{
    /// <summary>
    /// Container for holding ping result info to be added to the DB.
    /// </summary>
    public class ScanResult
    {

        public string DeviceGUID { get; set; }
        public string IP { get; set; }
        public bool Success { get; set; }
        public string Hostname { get; set; }

        public ScanResult(string deviceGUID, string ip, bool success, string hostname)
        {
            DeviceGUID = deviceGUID;
            IP = ip;
            Success = success;
            Hostname = hostname;
        }



    }
}
