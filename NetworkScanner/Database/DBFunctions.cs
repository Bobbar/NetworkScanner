﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using NetworkScanner.NetworkScanning;

namespace NetworkScanner.Database
{
    public static class DBFunctions
    {
        // Dictionary for caching device GUID & hostname.
        private static Dictionary<string, string> deviceGUIDCache;

        /// <summary>
        /// Add scan results to database, checking for changes from to recent IP. Returns number of rows affected.
        /// </summary>
        /// <param name="results"></param>
        /// <returns>Number of affected/inserted rows.</returns>
        public static bool InsertScanResults(List<ScanResult> results)
        {
            if (results.Count <= 0) return false;
            int affectedRows = 0;
            int insertedRows = 0;

            var db = DBFactory.GetDatabase();
            using (var trans = db.StartTransaction())
            {
                try
                {
                    foreach (ScanResult result in results)
                    {
                        if (!string.IsNullOrEmpty(result.DeviceGUID))
                        {
                            // Only add a new entry if the IP has changed from the most recent scan.
                            var lastip = LastIP(result.DeviceGUID);
                            if (result.IP != lastip)
                            {
                                Console.WriteLine("CHANGE: " + result.DeviceGUID + " - " + " from: " + lastip + " to: " + result.IP);
                                insertedRows++;
                                string insertQry = "INSERT INTO device_ping_history (device_guid, ip, hostname) VALUES ('" + result.DeviceGUID + "','" + result.IP + "','" + result.Hostname + "')";
                                var cmd = db.GetCommand(insertQry);
                                affectedRows += db.ExecuteQuery(cmd, trans);
                            }
                        }
                    }
                    if (affectedRows == insertedRows)
                    {
                        trans.Commit();
                        Console.WriteLine(affectedRows + " entries added.");
                        return true;
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    trans.Rollback();
                    return false;
                }
            }

        }

        /// <summary>
        /// Returns the IP of the previous most recent scan.
        /// </summary>
        /// <param name="deviceGUID"></param>
        /// <returns></returns>
        private static string LastIP(string deviceGUID)
        {
            try
            {
                string query = "SELECT ip FROM device_ping_history WHERE id = ( SELECT MAX(id) FROM device_ping_history WHERE device_guid = '" + deviceGUID + "')";

                string lastip = DBFactory.GetDatabase().ExecuteScalarFromQueryString(query).ToString();

                return lastip;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Finds the device GUID from hostname.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static string DeviceGUID(string hostname)
        {
            if (deviceGUIDCache.ContainsKey(hostname))
            {
                return deviceGUIDCache[hostname];
            }
            return string.Empty;
        }

        /// <summary>
        /// Initialized and populates the device GUID/Hostname cache.
        /// </summary>
        public static void PopulateGUIDCache()
        {
            deviceGUIDCache = new Dictionary<string, string>();
            string query = "SELECT dev_UID, dev_hostname FROM devices WHERE dev_hostname IS NOT NULL";

            try
            {
                using (var results = DBFactory.GetDatabase().DataTableFromQueryString(query))
                {
                    foreach (DataRow row in results.Rows)
                    {
                        deviceGUIDCache.Add(row["dev_hostname"].ToString(), row["dev_UID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Returns a list of all device hostnames located in the database.
        /// </summary>
        /// <returns></returns>
        public static List<string> Hostnames()
        {
            string query = "SELECT dev_hostname FROM devices WHERE dev_hostname IS NOT NULL";
            var tmpList = new List<string>();

            try
            {
                using (var results = DBFactory.GetDatabase().DataTableFromQueryString(query))
                {
                    foreach (DataRow row in results.Rows)
                    {
                        tmpList.Add(row["dev_hostname"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tmpList;
        }
    }

}