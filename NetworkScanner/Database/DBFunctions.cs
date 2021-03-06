﻿using NetworkScanner.NetworkScanning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

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
        public static bool InsertScanResults(List<ScanResult> results, int maxEntriesAllowed)
        {
            Logging.Debug("Starting DB insert...");
            if (results.Count <= 0) return false;
            int affectedRows = 0;
            int insertedRows = 0;
            Logging.Debug("Getting database...");
            var db = DBFactory.GetDatabase();
            Logging.Debug("Got database.");
            Logging.Debug("Starting transaction...");
            using (var trans = db.StartTransaction())
            {
                Logging.Debug("Transaction started.");
                try
                {
                    foreach (ScanResult result in results)
                    {
                        if (!string.IsNullOrEmpty(result.DeviceGUID))
                        {
                            // Trim # of historical entries.
                            TrimHistory(result.DeviceGUID, maxEntriesAllowed, trans);

                            // Add a new entry if the IP has not been previously recorded,
                            // or update the timestamp if the IP already exists.
                            if (HasIP(result.DeviceGUID, result.IP))
                            {
                                Logging.Verbose($@"UPDATE: { result.DeviceGUID } - { result.IP }");
                                insertedRows++;
                                string ipId = MostRecentIPIndex(result.DeviceGUID, result.IP);
                                string updateQry = $@"UPDATE device_ping_history SET timestamp = '{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }' WHERE device_guid = '{ result.DeviceGUID }' AND id = '{ ipId }'";
                                affectedRows += db.ExecuteNonQuery(updateQry, trans);
                            }
                            else
                            {
                                Logging.Verbose($@"ADD: { result.DeviceGUID } - to: { result.IP }");
                                insertedRows++;
                                string insertQry = $@"INSERT INTO device_ping_history (device_guid, ip, hostname) VALUES ('{ result.DeviceGUID }','{ result.IP }','{ result.Hostname }')";
                                affectedRows += db.ExecuteNonQuery(insertQry, trans);
                            }
                        }
                    }
                    if (affectedRows == insertedRows)
                    {
                        trans.Commit();
                        Logging.Verbose($@"{ affectedRows } entries added.");
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
                    Logging.Error(ex.ToString());
                    trans.Rollback();
                    return false;
                }
            }

        }

        /// <summary>
        /// Trims the number of ping history entries for the specified device.
        /// </summary>
        private static void TrimHistory(string deviceGUID, int maxEntries, DbTransaction trans)
        {
            if (maxEntries <= 0)
                return;

            // Select entries ordered by oldest to newest.
            string query = $@"SELECT * FROM device_ping_history WHERE device_guid = '{deviceGUID}' ORDER BY timestamp";

            using (DbCommand cmd = DBFactory.GetDatabase().GetCommand(query))
            using (DataTable results = DBFactory.GetDatabase().DataTableFromCommand(cmd, trans))
            {
                // Number of rows to remove to fit max count.
                int removeCount = results.Rows.Count - maxEntries;

                if (removeCount > 0)
                {
                    // Delete oldest entries.
                    for (int i = 0; i < removeCount; i++)
                    {
                        results.Rows[i].Delete();
                    }

                    // Verify results.
                    int removed = DBFactory.GetDatabase().UpdateTable(query, results, trans);
                    if (removed != removeCount)
                        throw new Exception($@"Unexpected number of rows deleted during trim operation.  Expected: {removeCount}  Result: {removed}");

                    Logging.Verbose($@"Removed {removeCount} entries from { results.Rows[0]["hostname"].ToString()}");
                }
            }
        }

        private static string MostRecentIPIndex(string deviceGUID, string ip)
        {
            string query = $@"SELECT id, device_guid, ip FROM device_ping_history WHERE device_guid = '{ deviceGUID }' AND ip ='{ ip }' ORDER BY id DESC LIMIT 1";

            string id = DBFactory.GetDatabase().ExecuteScalarFromQueryString(query).ToString();

            if (!string.IsNullOrEmpty(id))
            {
                return id;
            }

            return string.Empty;
        }


        private static bool HasIP(string deviceGUID, string ip)
        {
            string query = $@"SELECT id, device_guid, ip FROM device_ping_history WHERE device_guid = '{ deviceGUID }' AND ip ='{ ip }'";

            using (var results = DBFactory.GetDatabase().DataTableFromQueryString(query))
            {
                if (results.Rows.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the IP of the previous most recent scan.
        /// </summary>
        /// <param name="deviceGUID"></param>
        /// <returns></returns>
        private static string PreviousIP(string deviceGUID)
        {
            try
            {
                string query = $@"SELECT ip FROM device_ping_history WHERE id = ( SELECT MAX(id) FROM device_ping_history WHERE device_guid = '{ deviceGUID }')";

                string prevIp = DBFactory.GetDatabase().ExecuteScalarFromQueryString(query).ToString();

                return prevIp;
            }
            catch (Exception ex)
            {
                Logging.Error(ex.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// Finds the device GUID from hostname using a dictionary cache.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static string DeviceGUID(string hostname)
        {
            if (deviceGUIDCache == null)
            {
                Logging.Debug("Populating GUID Cache...");
                PopulateGUIDCache();
                Logging.Debug("Done.");
            }


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
            Logging.Debug("Initing GUID Dictionary...");
            deviceGUIDCache = new Dictionary<string, string>();
            Logging.Debug("Dictionary initiated.");

            string query = "SELECT dev_UID, dev_hostname FROM devices WHERE dev_hostname IS NOT NULL";

            try
            {
                Logging.Debug("Getting DB Results...");
                using (var results = DBFactory.GetDatabase().DataTableFromQueryString(query))
                {
                    Logging.Debug("Iterate rows...");
                    foreach (DataRow row in results.Rows)
                    {
                        deviceGUIDCache.Add(row["dev_hostname"].ToString(), row["dev_UID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.ToString());
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
                Logging.Error(ex.ToString());
            }
            return tmpList;
        }

    }

}
