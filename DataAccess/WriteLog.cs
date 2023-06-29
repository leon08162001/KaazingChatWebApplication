using System;
using System.Data;
using System.Data.SqlClient;
//using System.Data.OracleClient;

namespace DataAccess.DB
{
    /// <summary>
    /// WriteLog 的摘要描述。
    /// </summary>
    public class WriteLog
    {
        private Dac db;
        private dbkind Datatype;
        private string ConnStr;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteLog"/> class.
        /// </summary>
        public WriteLog(dbkind dbkind, string ConnString)
        {
            db = new Dac(dbkind);
            Datatype = dbkind;
            ConnStr = ConnString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteLog"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public WriteLog(Exception ex)
        {
            string SPName;
            if (ex.GetType().Name == "SqlException")
            {
                SPName = ((SqlException)ex).Procedure;
            }
            //else if (ex.GetType().Name == "OracleException")
            //{
            //    SPName = ((OracleException)ex).Message;
            //}
            else { SPName = ""; }
            string StrSQL;
            StrSQL = "Insert Into [Exp]([ExpType],[SPName],[ErrMsg],[StackTrace],[Createdate]) Values('" + ex.GetType().Name.Replace("'", "''") + "','" + SPName.Replace("'", "''") + "','" + ex.Message.Replace("'", "''") + "','" + ex.StackTrace.Replace("'", "''") + "',getdate())";
            if (Datatype == dbkind.SQL_Server)
            {
                SqlConnection con = new SqlConnection(ConnStr);
                con.Open();
                SqlCommand command = new SqlCommand(StrSQL, con);
                command.ExecuteNonQuery();
                con.Close();
            }
            //else if (Datatype == dbkind.Oracle)
            //{
            //    OracleConnection con = new OracleConnection(ConnStr);
            //    con.Open();
            //    OracleCommand command = new OracleCommand(StrSQL, con);
            //    command.ExecuteNonQuery();
            //    con.Close();
            //}
        }

        /// <summary>
        /// 記錄意外狀況
        /// </summary>
        /// <param name="ex">Exception 物件</param>
        public void LogExp(Exception ex)
        {
            string SPName;
            if (ex.GetType().Name == "SqlException")
            {
                SPName = ((SqlException)ex).Procedure;
            }
            else { SPName = ""; }
            string StrSQL;
            int Roa = 0;
            StrSQL = "Insert Into [Exp]([ExpType],[SPName],[ErrMsg],[StackTrace],[Createdate]) Values('" + ex.GetType().Name.Replace("'", "''") + "','" + SPName.Replace("'", "''") + "','" + ex.Message.Replace("'", "''") + "','" + ex.StackTrace.Replace("'", "''") + "',getdate())";
            db.RunSQL(StrSQL, out Roa, "cmdSQL");
        }

        //		public static void LogExp(string FileName, Exception ex)
        //		{
        //			StreamWriter sw=new StreamWriter(FileName,true,System.Text.Encoding.Default);
        //			sw.WriteLine(ex.GetType().Name.Replace("'","''") + "," + ex.Message.Replace("'","''") + "," +  ex.StackTrace.Replace("'","''") + "," +DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
        //			sw.Close();
        //		}

        /// <summary>
        ///記錄資料庫操作
        ///記錄被執行的SQL指令
        /// </summary>
        /// <param name="cmdSQL">The CMD SQL.</param>
        public void LogDB(string cmdSQL)
        {
            string strSQL;
            int Roa;
            strSQL = "Insert Into [AccessLog] Values('','" + cmdSQL.Replace("'", "''") + "','','" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "')";
            db.RunSQL(strSQL, out Roa, ConnStr);
        }


        //記錄目前使用者及SQL指令
        /// <summary>
        /// Logs the DB.
        /// </summary>
        /// <param name="cmdSQL">The CMD SQL.</param>
        /// <param name="currentUser">The current user.</param>
        public void LogDB(string cmdSQL, string currentUser)
        {
            string strSQL;
            int Roa;
            strSQL = "Insert Into [AccessLog] Values('" + currentUser + "','" + cmdSQL.Replace("'", "''") + "','','" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "')";
            db.RunSQL(strSQL, out Roa, ConnStr);
        }


        //記錄使用者,應用程式所在路徑及SQL指令
        /// <summary>
        /// Logs the DB.
        /// </summary>
        /// <param name="cmdSQL">The CMD SQL.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="AppPath">The app path.</param>
        public void LogDB(string cmdSQL, string currentUser, string AppPath)
        {
            string strSQL;
            int Roa;
            strSQL = "Insert Into [AccessLog] Values('" + currentUser + "','" + cmdSQL.Replace("'", "''") + "','" + AppPath + "','" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "')";
            db.RunSQL(strSQL, out Roa, ConnStr);
        }


        //取得例外資料列表
        /// <summary>
        /// Gets the exp list.
        /// </summary>
        /// <returns></returns>
        public DataTable GetExpList()
        {
            DataTable dt = null;
            string strSQL = "";
            strSQL = "select * from [exp] Order By CreateDate";
            dt = db.RunSQL(strSQL, "ExpList", ConnStr);
            return dt;
        }


        //取得資料庫存取資料列表
        /// <summary>
        /// Gets the DB access list.
        /// </summary>
        /// <returns></returns>
        public DataTable GetDBAccessList()
        {
            DataTable dt = null;
            string strSQL = "";
            strSQL = "select * from [AccessLog] Order By CreateDate";
            dt = db.RunSQL(strSQL, "ExpList", ConnStr);
            return dt;
        }
    }
}