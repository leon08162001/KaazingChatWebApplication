using Common;
using Common.Utility;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KaazingChatWebApplication
{
    public partial class WebChat_WebCam1 : System.Web.UI.Page
    {
        //IApplicationContext applicationContext;
        //Config config;

        protected string ClientIp = "";
        protected string KaazingJmsSvc = "";
        protected bool IsSaveVideoStreamToServer = false;
        protected string EnCryptWebSocketUID = "";
        protected string EnCryptWebSocketPWD = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            //applicationContext = ContextRegistry.GetContext();
            //config = (Config)applicationContext.GetObject("Config");
            //KaazingJmsSvc = config.IsUseSSL ? 
            //                "wss://" + config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms" : 
            //                "ws://" + config.KaazingWebSocket_network + ":" + config.KaazingWebSocket_service + "/jms";
            IsSaveVideoStreamToServer = Config.IsSaveVideoStreamToServer;
            GetWebSocketLoadBalancerUrl();
            ClientIp = GetClientIp();
            EnCryptWebSocketUID = Config.KaazingWebSocketUserID;
            EnCryptWebSocketPWD = Config.KaazingWebSocketPwd;
        }
        private string GetClientIp()
        {
            string ip = "";

            ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
        private void GetWebSocketLoadBalancerUrl()
        {
            try
            {
                List<string> LoadBalancerUrls = new List<string>();
                if (Config.KaazingWebSocket_network.IndexOf(",") != -1)
                {
                    foreach (var ip in Config.KaazingWebSocket_network.Split(new char[] { ',' }))
                    {
                        LoadBalancerUrls.Add(ip + ":" + Config.KaazingWebSocket_service);
                    }
                }
                else
                {
                    LoadBalancerUrls.Add(Config.KaazingWebSocket_network + ":" + Config.KaazingWebSocket_service);
                }
                foreach (var lbUrl in LoadBalancerUrls)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        string ip = lbUrl.IndexOf(':') != -1 && lbUrl.Split(new char[] { ':' }).Length == 2 ? lbUrl.Split(new char[] { ':' })[0] : "";
                        string port = lbUrl.IndexOf(':') != -1 && lbUrl.Split(new char[] { ':' }).Length == 2 ? lbUrl.Split(new char[] { ':' })[1] : "";
                        int n;
                        bool chkPort = int.TryParse(port, out n);
                        if (!chkPort || (ip.Equals("") || port.Equals("")))
                        {
                            KaazingJmsSvc = "";
                            break;
                        }
                        try
                        {
                            tcpClient.Connect(ip, Convert.ToInt16(port));
                        }
                        catch (Exception ex)
                        {
                        }
                        if (tcpClient.Connected)
                        {
                            tcpClient.Close();
                            KaazingJmsSvc = Config.IsUseSSL ? "wss://" + ip + ":" + port + "/jms" : "ws://" + ip + ":" + port + "/jms";
                            break;
                        }
                        KaazingJmsSvc = "";
                    }
                }
            }
            catch (Exception ex)
            {
                KaazingJmsSvc = "";
            }
        }
    }
}