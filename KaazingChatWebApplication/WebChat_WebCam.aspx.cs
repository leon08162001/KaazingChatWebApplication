using Common;
using Common.Utility;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KaazingTestWebApplication.Controllers;
using KaazingChatWebApplication.Models;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using System.Net.Http;
using System.Configuration;
using Spring.Expressions.Parser.antlr;

namespace KaazingChatWebApplication
{
    public partial class WebChat_WebCam : System.Web.UI.Page
    {
        //IApplicationContext applicationContext;
        //Config config;

        protected string ClientIp = "";
        protected string KaazingJmsSvc = "";
        protected bool IsSaveVideoStreamToServer = false;
        protected string EnCryptWebSocketUID = "";
        protected string EnCryptWebSocketPWD = "";
        protected string timeStamp = HttpContext.Current.Timestamp.ToString("yyyyMMddHHmmssfff");
        String authenticationLoginServiceUrl = "api/Authenticate/Login";

        protected async void Page_Load(object sender, EventArgs e)
        {
            FileInfo IniFile = new FileInfo(Path.Combine(Server.MapPath(""), "common.ini"));
            using (FileStream FS = IniFile.OpenRead())
            {
                Config.ConfigStream = FS;
                Config.ReadParameter();
                Common.LogHelper.Logger.logPath = Path.Combine(Server.MapPath(""), Config.logDir);
            }

            if (Request.Form["login"] != null)
            {
                listenFrom.Value = Request.Form["login"].ToString().ToUpper();
                listenFrom.Disabled = true;
                //WebChatController Wcc = new WebChatController();
                //Chat chat = new Chat();
                //chat.id = listenFrom.Value.Trim().ToUpper();
                //chat.name = listenFrom.Value.Trim().ToUpper();
                //IHttpActionResult result = Wcc.GetAllTalkFriends(chat);
                //if (result is OkNegotiatedContentResult<List<Chat>>)
                //{
                //    // Here's how you can do it. 
                //    List<Chat> friendsListForChat = (result as OkNegotiatedContentResult<List<Chat>>).Content;
                //    ddlAllFriends.Items.Clear();
                //    foreach (Chat friend in friendsListForChat)
                //    {
                //        ddlAllFriends.Items.Add(new ListItem(friend.receiver, friend.receiver));
                //    }
                //    ddlAllFriends.SelectedIndex = 0;
                //    talkTo.Value = ddlAllFriends.SelectedValue;
                //}
            }
            else
            {
                listenFrom.Value = "";
                listenFrom.Disabled = false;
            }
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "GetAllTalkFriends", "GetAllTalkFriends();", true);

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

            //取得存取API的Token存入httpclient header Authorization
            string accessToken = await GetAccessToken("leon", "1qaz!QAZ");
            if (accessToken.Equals(""))
            {
                ClientScript.RegisterClientScriptBlock(this.GetType(), "accessTokenLoss", "alert('權限不足無法取得聊天功能API的Token，系統功能無法使用')", true);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(this.GetType(), "accessToken", "sessionStorage.setItem('accessToken','" + accessToken + "');", true);
            }
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
        private async Task<string> GetAccessToken(string userName, string userPwd)
        {
            var accessToken = "";
            var dict = new Dictionary<string, string>();
            dict.Add("username", userName);
            dict.Add("password", userPwd);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebChatApiUrl"]);
                var content = new FormUrlEncodedContent(dict);
                var response = await client.PostAsync(authenticationLoginServiceUrl, content);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    dynamic tokenObj = JsonConvert.DeserializeObject(result);
                    accessToken = tokenObj["Token"];
                }
            }
            return accessToken;
        }
    }
}