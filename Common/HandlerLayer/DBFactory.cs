using DataAccess.DB;
using System;
using System.Data;

namespace Common.HandlerLayer
{
    public static class DBFactory
    {
        //private static Dac DB = new Dac(dbtype.Oracle, "LOCALOMS");
        public static DataTable DoDBTask(TopicType TopicType, DataTable DT)
        {
            switch (TopicType)
            {
                case TopicType.JefferiesExcuReport :
                    return DoJefferiesExcuReport(DT);
                case TopicType.OTAExport :
                    return DoOTAExport(DT);
                case TopicType.OTA1Export:
                    return DoOTA1Export(DT);
                case TopicType.OTA2Export:
                    return DoOTA2Export(DT);
                default :
                    return new DataTable();
            }
        }

        private static DataTable DoJefferiesExcuReport(DataTable DT)
        {
            try
            {
                Dac DB = new Dac(dbtype.SQL_Server, "LOCALOMS");
                //Dac DB = new Dac(dbtype.Oracle, "LOCALOMS");
                DataTable UserDT;
                string MessageID = "";
                if (DT.Rows.Count > 0)
                {
                    MessageID = DT.Rows[0]["MessageID"].ToString();
                }
                //lock (DB)
                //{
                    string Sql = DT.Rows[0]["Sql"].ToString();
                    UserDT = DB.RunSQL(Sql, "App_user", "");
                //}
                //UserDT.Columns.Add("MessageID", typeof(System.String));
                //if (UserDT.Rows.Count > 0)
                //{
                //    UserDT.Rows[0]["MessageID"] = MessageID;
                //}
                //test
                //for (int i = 0; i < 100000; i++)
                //{
                //    string s = "";
                //    s = "0" + i.ToString();
                //}
                return UserDT;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static DataTable DoOTAExport(DataTable DT)
        {
            try
            {
                Dac DB = new Dac(dbtype.SQL_Server, "LOCALOMS");
                //Dac DB = new Dac(dbtype.Oracle, "LOCALOMS");
                DataTable UserDT;
                string MessageID = "";
                if (DT.Rows.Count > 0)
                {
                    MessageID = DT.Rows[0]["MessageID"].ToString();
                }
                //lock (DB)
                //{
                    string Sql = DT.Rows[0]["Sql"].ToString();
                    UserDT = DB.RunSQL(Sql, "App_user", "");
                //}
                //UserDT.Columns.Add("MessageID", typeof(System.String));
                //if (UserDT.Rows.Count > 0)
                //{
                //    UserDT.Rows[0]["MessageID"] = MessageID;
                //}
                //test
                //for (int i = 0; i < 100000; i++)
                //{
                //    string s = "";
                //    s = "0" + i.ToString();
                //}
                return UserDT;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static DataTable DoOTA1Export(DataTable DT)
        {
            try
            {
                Dac DB = new Dac(dbtype.SQL_Server, "LOCALOMS");
                //Dac DB = new Dac(dbtype.Oracle, "LOCALOMS");
                DataTable UserDT;
                string MessageID = "";
                if (DT.Rows.Count > 0)
                {
                    MessageID = DT.Rows[0]["MessageID"].ToString();
                }
                //lock (DB)
                //{
                    string Sql = DT.Rows[0]["Sql"].ToString();
                    UserDT = DB.RunSQL(Sql, "App_user", "");
                //}
                UserDT.Columns.Add("MessageID", typeof(System.String));
                if (UserDT.Rows.Count > 0)
                {
                    UserDT.Rows[0]["MessageID"] = MessageID;
                }
                //test
                //for (int i = 0; i < 50000; i++)
                //{
                //    string s = "";
                //    s = "0" + i.ToString();
                //}
                return UserDT;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private static DataTable DoOTA2Export(DataTable DT)
        {
            try
            {
                Dac DB = new Dac(dbtype.SQL_Server, "LOCALOMS");
                //Dac DB = new Dac(dbtype.Oracle, "LOCALOMS");
                DataTable UserDT;
                string MessageID = "";
                if (DT.Rows.Count > 0)
                {
                    MessageID = DT.Rows[0]["MessageID"].ToString();
                }
                //lock (DB)
                //{
                    string Sql = DT.Rows[0]["Sql"].ToString();
                    UserDT = DB.RunSQL(Sql, "App_user", "");
                //}
                UserDT.Columns.Add("MessageID", typeof(System.String));
                if (UserDT.Rows.Count > 0)
                {
                    UserDT.Rows[0]["MessageID"] = MessageID;
                }
                //test
                //for (int i = 0; i < 50000; i++)
                //{
                //    string s = "";
                //    s = "0" + i.ToString();
                //}
                return UserDT;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
