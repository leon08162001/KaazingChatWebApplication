using Kaazing.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Common.LinkLayer
{
    ///
    /// Challenge handler for Basic authentication. See RFC 2617.
    ///
    public class KaazingSocketLogin : LoginHandler
    {
        private string _UserID = "";
        private string _Password = "";
        private const int maxRetries = 10;
        private int retry = 0;
        private string loginMsg = "";
        public KaazingSocketLogin(string UserID, string Password)
        {
            _UserID = UserID;
            _Password = Password;
        }
        public string LoginMsg
        {
            get { return loginMsg; }
        }
        PasswordAuthentication LoginHandler.GetCredentials()
        {
            //if (retry ++ >= maxRetries)
            //{
            //    loginMsg = "UserID Or Password used to connect to Kaazing Websocket is incorrect";
            //    return null;      // abort authentication process when max retry reached
            //}
            //PasswordAuthentication credentials = new PasswordAuthentication(_UserID, _Password.ToCharArray());
            //return credentials;
            return AuthenticationHandler();
        }

        private PasswordAuthentication AuthenticationHandler()
        {
            PasswordAuthentication credentials = null;
            AutoResetEvent userInputCompleted = new AutoResetEvent(false);

            //
            //Popup login window on new Task. Note: please use Task to do parallel jobs
            //
            Task t = Task.Factory.StartNew(() =>
            {
                credentials = new PasswordAuthentication(_UserID, _Password.ToCharArray());
                userInputCompleted.Set();
            });

            // wait user click 'OK' or 'Cancel' on login window
            userInputCompleted.WaitOne();
            return credentials;
        }
    }
}
