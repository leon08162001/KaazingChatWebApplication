using Apache.NMS;
using Apache.NMS.ActiveMQ;
//using Apache.NMS.Stomp;
using Common.Utility;

namespace Common.LinkLayer
{
    public class AMQSharedConnection
    {
        //protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static IConnectionFactory _Factory = null;
        protected static IConnection _Connection = null;
        public static IConnection GetConnection()
        {
            return _Connection;
        }

        public static void Open(string serverUrl, string serverPort, string userName, string passWord, bool useSSL = false, bool IsDurableConsumer = false, string ClientID = "")
        {
            //if (_Connection == null)
            //{
            _Factory = new ConnectionFactory(Util.GetMQFailOverConnString(serverUrl, serverPort, useSSL));
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
            catch (NMSException ex)
            {
                Common.LogHelper.Logger.LogError<AMQSharedConnection>(ex);
                throw ex;
            }
            try
            {
                if (IsDurableConsumer && !string.IsNullOrEmpty(ClientID)) _Connection.ClientId = ClientID;
                _Connection.Start();
            }
            catch (NMSException ex)
            {
                Common.LogHelper.Logger.LogError<AMQSharedConnection>(ex);
                throw ex;
            }
            //}
        }
        public static void Close()
        {
            if (_Connection != null && _Connection.IsStarted)
            {
                _Connection.Stop();
                _Connection.Close();
                _Connection = null;
            }
        }
    }
}
