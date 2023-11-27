using KaazingChatApi;
using System.Data;
using System.Data.SqlClient;

namespace KaazingChatWebApplication.Connection
{
    public class ConnectionFactory
    {
        public IDbConnection CreateConnection(string name = "default")
        {
            switch (name)
            {
                case "default":
                    {
                        var ConnectionString = Startup.AppSettingManager.GetConnectionString("default");
                        return new SqlConnection(ConnectionString);
                    }
                default:
                    {
                        var ConnectionString = Startup.AppSettingManager.GetConnectionString(name);
                        return new SqlConnection(ConnectionString);
                    }
            }
        }
    }
}