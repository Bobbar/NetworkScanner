using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetworkScanner.NetworkScanning
{
    public class Pinger
    {
        public event EventHandler PingComplete;

        private const int timeOut = 1000;
        private Ping myPinger;
        private string hostName;
        private PingReply pingReply;

        public Pinger(string hostname)
        {
            this.hostName = hostname;
            myPinger = new Ping();
        }

        protected virtual void OnPingComplete(PingerCompleteEventArgs e)
        {
            PingComplete(this, e);
            myPinger.Dispose();
        }

        public string PingIP
        {
            get
            {
                return hostName;
            }
        }

        public async void StartPing()
        {
            bool success;
            string ip = "";
            try
            {
                pingReply = await GetPingReply(hostName);
                ip = pingReply.Address.ToString();
                if (pingReply.Status == IPStatus.Success)
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Logging.Warning(hostName + " - " + ex.Message);
                success = false;
            }
            OnPingComplete(new PingerCompleteEventArgs(success, hostName, ip));
        }

        private async Task<PingReply> GetPingReply(string ip)
        {
            var pingOptions = new PingOptions();
            pingOptions.DontFragment = true;
            byte[] pingBuffer = Encoding.ASCII.GetBytes("ping");

            return await myPinger.SendPingAsync(ip, timeOut, pingBuffer, pingOptions);
        }

        public class PingerCompleteEventArgs : EventArgs
        {
            public bool Success { get; }
            public string Hostname { get; }
            public string PingIP { get; }

            public PingerCompleteEventArgs(bool success, string hostname, string pingIp)
            {
                Success = success;
                Hostname = hostname;
                PingIP = pingIp;
            }
        }

    }
}

