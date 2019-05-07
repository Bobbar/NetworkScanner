using Databases.Data;
namespace NetworkScanner.Database
{
    public static class DBFactory
    {
        public static IDatabase GetDatabase()
        {
            //if (GlobalSwitches.CachedMode)
            //{
            //    return new SQLiteDatabase(false);
            //}
            //else
            //{
            return new MySqlDatabase("10.10.0.89", "netscanuser", "netscanpassword", "asset_manager");
            //}
        }
    }
}
