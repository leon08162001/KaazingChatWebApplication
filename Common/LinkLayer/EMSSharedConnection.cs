using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TIBCO.EMS;

namespace Common.LinkLayer
{
    [Serializable]

    public class EMSSharedConnection
    {
        //protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static ConnectionFactory _Factory = null;
        protected static Connection _Connection = null;

        public static Connection GetConnection()
        {
            return _Connection;
        }
        public static void Open(string serverUrl, string serverPort, string userName, string passWord, bool useSSL = false, List<string> CertsPath = null, bool IsDurableConsumer = false, string ClientID = "")
        {
            //if (_Connection == null)
            //{
            _Factory = new ConnectionFactory(Util.GetEMSFailOverConnString(serverUrl, serverPort, useSSL));
            _Factory.SetReconnAttemptCount(1200);   // 1200retries
            _Factory.SetReconnAttemptDelay(5000);  // 5seconds
            _Factory.SetReconnAttemptTimeout(20000); // 20seconds
            if (useSSL)
            {
                SSLSetting(ref _Factory, serverUrl, CertsPath);
                //EMSSSLFileStoreInfo storeInfo = new EMSSSLFileStoreInfo();
                //_Factory.SetTargetHostName(serverUrl);
                //storeInfo.SetSSLTrustedCertificate(@"C:\ProgramData\TIBCO_HOME\tibco\cfgmgmt\ems\certs\server.cert.cer");
                //_Factory.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE, storeInfo);
            }
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
                Common.LogHelper.Logger.LogError<EMSSharedConnection>(ex);
                throw ex;
            }
            try
            {
                _Connection.Start();
            }
            catch (TIBCO.EMS.EMSException ex)
            {
                Common.LogHelper.Logger.LogError<EMSSharedConnection>(ex);
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
        public static void SSLSetting(ref ConnectionFactory ConnectionFactory, string Urls, List<string> CertificatesPath)
        {
            EMSSSLFileStoreInfo storeInfo = new EMSSSLFileStoreInfo();
            //代表只有1個IP
            if (Urls.IndexOf(",") == -1)
            {
                ConnectionFactory.SetTargetHostName(Urls);
                if (CertificatesPath != null)
                {
                    //storeInfo.SetSSLTrustedCertificate(CertificatesPath[0]);
                    foreach (string CertPath in CertificatesPath)
                    {
                        storeInfo.SetSSLTrustedCertificate(CertPath);
                    }
                }
                ConnectionFactory.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE, storeInfo);
            }
            //代表多個IP
            else
            {
                List<string> urls = Urls.Split(new char[] { ',' }).ToList<string>();
                ConnectionFactory.SetTargetHostName(urls[0]);
                if (CertificatesPath != null)
                {
                    foreach (string CertPath in CertificatesPath)
                    {
                        storeInfo.SetSSLTrustedCertificate(CertPath);
                    }
                }
                ConnectionFactory.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE, storeInfo);
            }
        }
    }
}
