using System;
using System.Data;
using System.Data.SqlClient;

namespace DataAccess.DB
{
    /// <summary>
    ///		SQL Transaction功能元件 
    /// </summary>
    public class Transaction : IDisposable
    {
        private SqlConnection Conn;
        private SqlCommand Cmd;
        private SqlTransaction Trans;

        /// <summary>
        /// <see cref="Transaction"/> class 建構子
        /// </summary>
        /// <param name="Connection">The connection.</param>
        public Transaction(string Connection)
        {
            Conn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings[Connection]);
        }

        /// <summary>
        /// <see cref="Transaction"/> class 建構子
        /// </summary>
        public Transaction()
        {
            Conn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings["Default"]);
        }


        /// <summary>
        /// 加入欲執行的T-SQL語法至transaction
        /// </summary>
        /// <param name="strSQL">T-SQL 語法</param>
        /// <param name="Commit">是否結束Transaction</param>
        /// <returns></returns>
        /// <example>
        /// AddTransaction使用範例：
        /// <code>
        /// string strSQL1 = "Insert into region values(2,'en')";
        /// string strSQL2 = "Delete region where regionid=1";
        /// AddTransaction(strSQL1,false);  //加入第一段欲執行的SQL語法至Transaction元件
        /// if(AddTransaction(strSQL2,true)) //加入最後一段欲執行的SQL語法，並設定Commit為True
        /// {	//執行成功
        /// }
        /// else
        /// {	//執行失敗
        /// }
        /// </code>
        /// </example>
        public bool AddTransaction(string strSQL, bool Commit)
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Close();
                Conn.Open();
                Trans = Conn.BeginTransaction();
            }
            try
            {
                new SqlCommand(strSQL, Conn, Trans)
                    .ExecuteNonQuery();
                if (Commit)
                {
                    Trans.Commit();
                    Conn.Close();
                }
                return true;
            }
            catch (Exception Ex)
            {
                Trans.Rollback();
                return false;
            }
        }

        #region IDisposable 成員

        /// <summary>
        /// 執行與釋放 (Free)、釋放 (Release) 或重設 Unmanaged 資源相關聯之應用程式定義的工作。
        /// </summary>
        public void Dispose()
        {
            if (Conn != null)
            {
                Conn.Close();
                Conn = null;
            }
        }

        #endregion
    }
}