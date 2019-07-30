using Databases.Data;
namespace NetworkScanner.Database
{
    public static class DBFactory
    {
        public static IDatabase GetDatabase()
        {
            return new MySqlDatabase("10.10.0.89", "netscanuser", "netscanpassword", "asset_manager");
        }
    }
}
