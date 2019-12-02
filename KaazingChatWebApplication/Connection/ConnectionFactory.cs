using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

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
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["SQLConnectionString"].ConnectionString;
                        return new SqlConnection(ConnectionString);
                    }
                default:
                    {
                        var ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings[name].ConnectionString;
                        return new SqlConnection(ConnectionString);
                    }
            }
        }
    }
}