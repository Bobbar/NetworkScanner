using System;
using System.Collections.Generic;
using System.Text;
using NetworkScanner.Database;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;


namespace NetworkScanner.NetworkScanning
{
    public static class MultiThreadScanner
    {
        private static List<ScanResult> pingResults = new List<ScanResult>();
        private static List<string> hostsList;
        private static List<Task<List<ScanResult>>> taskList = new List<Task<List<ScanResult>>>();

        public static void StartScan(int threads)
        {
            var startTime = DateTime.Now.Ticks;

            Console.WriteLine("\n \n");
            Logging.Log("Starting Network Scan. Threads = " + threads);

            // Populate the GUID cache dictionary.
            DBFunctions.PopulateGUIDCache();

            Logging.Log("Getting Hostnames...");
            hostsList = DBFunctions.Hostnames();
            Logging.Log("Done.");

            Logging.Info(hostsList.Count.ToString() + " hostnames will be scanned.");

            // Calculate the number of hosts each thread will ping.
            int hostsPerThread = Math.Abs(hostsList.Count / threads);

            // Add any remainders to the count for the last thread.
            int lastThreadHostCount = hostsPerThread + (hostsList.Count - (hostsPerThread * threads));

            // Split up the hosts list and instantiate ping workers for the first threads (tasks).
            var startIndex = 0;
            for (int i = 0; i < threads - 1; i++)
            {
                var threadHosts = hostsList.GetRange(startIndex, hostsPerThread);
                startIndex += hostsPerThread;

                var newWorker = new PingWorker(threadHosts, i);
                taskList.Add(Task.Factory.StartNew(() => newWorker.GetResults()));
            }

            // Instantiate the last ping worker with the remaining hosts.
            var lastThreadHosts = hostsList.GetRange(startIndex, lastThreadHostCount);
            var lastWorker = new PingWorker(lastThreadHosts, threads);
            taskList.Add(Task.Factory.StartNew(() => lastWorker.GetResults()));

            // Wait for all workers to finish and collect results.
            var resultTask = Task.WhenAll(taskList.ToArray());
            resultTask.Wait();

            // Collect the results from each worker and merge them into a complete list.
            foreach (var result in resultTask.Result)
            {
                pingResults.AddRange(result);
            }

            Logging.Log("Scan Complete.  Num of results: " + pingResults.Count.ToString());
            Logging.Log("Inserting changes into database...");

            // Add all the successful results to the DB.
            if (DBFunctions.InsertScanResults(pingResults))
            {
                Logging.Log("Done!");
            }
            else
            {
                Logging.Log("Failed!");
            }
            var elapTime = (((DateTime.Now.Ticks - startTime) / 10000) / 1000);
            Logging.Log("Runtime: " + elapTime + " s");

        }
    }
}
