using Common.Utility;
using System;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    [Serializable]

    public class EMSSharedConnection
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static ConnectionFactory _Factory = null;
        protected static Connection _Connection = null;

        public static Connection GetConnection()
        {
            return _Connection;
        }
        public static void Open(string serverUrl, string serverPort, string userName, string passWord, bool useSSL = false, bool IsDurableConsumer = false, string ClientID = "")
        {
            //if (_Connection == null)
            //{
            _Factory = new ConnectionFactory(Util.GetEMSFailOverConnString(serverUrl, serverPort, useSSL));
            _Factory.SetReconnAttemptCount(1200);   // 1200retries
            _Factory.SetReconnAttemptDelay(5000);  // 5seconds
            _Factory.SetReconnAttemptTimeout(20000); // 5seconds
            if (IsDurableConsumer && !string.IsNullOrEmpty(ClientID)) _Factory.SetClientID(ClientID);
            try
            {
                if (userName != "" && passWord != "")
                {
                    _Connection = _Factory.CreateConnection(userName, passWord);
                }
                else
                {
                    _Connection = _Factory.CreateConnection();
                }
            }
            catch (TIBCO.EMS.EMSException ex)
            {
                if (log.IsErrorEnabled) log.Error("EMSSharedConnection Oepn() Error", ex);
                throw ex;
            }
            try
            {
                _Connection.Start();
            }
            catch (TIBCO.EMS.EMSException ex)
            {
                if (log.IsErrorEnabled) log.Error(string.Format("EMSSharedConnection Open({0},{1},{2}) Error", serverUrl, userName, passWord), ex);
                throw ex;
            }
            //}
        }

        public static void Close()
        {
            if (_Connection != null && !_Connection.IsClosed)
            {
                _Connection.Stop();
                _Connection.Close();
                _Connection = null;
            }
        }
    }
}
