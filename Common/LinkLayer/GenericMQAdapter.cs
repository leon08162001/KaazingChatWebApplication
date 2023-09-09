using Apache.NMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Common.LinkLayer
{
    [Serializable]

    public class GenericMQAdapter : BaseMQAdapter
    {
        protected Dictionary<string, string> _DicDataType = new Dictionary<string, string>();
        protected DataTable MessageDT = new DataTable(); //將Message資料轉換成DataTable所使用的DataTable

        private static GenericMQAdapter singleton;

        public GenericMQAdapter() : base() { }

        public GenericMQAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
            : base(Uri, DestinationFeature, ListenName, SendName) { }

        public GenericMQAdapter(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
            : base(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd) { }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static GenericMQAdapter getSingleton()
        {
            if (singleton == null)
            {
                singleton = new GenericMQAdapter();
            }
            return singleton;
        }

        public static GenericMQAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName)
        {
            if (singleton == null)
            {
                singleton = new GenericMQAdapter(Uri, DestinationFeature, ListenName, SendName);
            }
            return singleton;
        }

        public static GenericMQAdapter getSingleton(string Uri, DestinationFeature DestinationFeature, string ListenName, string SendName, string UserName, string Pwd)
        {
            if (singleton == null)
            {
                singleton = new GenericMQAdapter(Uri, DestinationFeature, ListenName, SendName, UserName, Pwd);
            }
            return singleton;
        }

        public override void processMessage(IMessage message)
        {
            string Message = "";
            string _ErrMsg = "";
            if (_DataType.Equals(typeof(String)))
            {
                ITextMessage msg = message as ITextMessage;
                DataTable ResultTable = new DataTable();
                ResultTable.Columns.Add("message");
                DataRow dr = ResultTable.NewRow();
                dr[0] = msg.Text;
                ResultTable.Rows.Add(dr);
                if (UISyncContext != null && IsEventInUIThread)
                {
                    UISyncContext.Post(OnMessageHandleFinished, new MQMessageHandleFinishedEventArgs(_ErrMsg, dr));
                }
                else
                {
                    OnMessageHandleFinished(new MQMessageHandleFinishedEventArgs(_ErrMsg, dr));
                }
                return;
            }
            Dictionary<string, string> MessageDictionary = new Dictionary<string, string>();
            foreach (object key in message.Properties.Keys)
            {
                MessageDictionary.Add(key.ToString(), message.Properties[key.ToString()].ToString());
            }
            if (MessageDictionary.Keys.Count == 0)
            {
                return;
            }
            foreach (string key in MessageDictionary.Keys)
            {
                Message += key + "=" + MessageDictionary[key] + ";";
            }
            //0.檢查是否為HeartBeat訊息,若是則忽略不處理
            if (MessageDictionary.ContainsKey("0"))
            {
                return;
            }
            //1.檢查是否有指定DataType,以便與傳過來的FixData作驗證用
            if (_DataType == null)
            {
                _ErrMsg = "not yet assigned Fix Tag Type of Fix Data";
                Common.LogHelper.Logger.LogInfo<GenericMQAdapter>(_ErrMsg);
                if (UISyncContext != null && IsEventInUIThread)
                {
                    UISyncContext.Post(OnMessageHandleFinished, new MQMessageHandleFinishedEventArgs(_ErrMsg, null));
                }
                else
                {
                    OnMessageHandleFinished(new MQMessageHandleFinishedEventArgs(_ErrMsg, null));
                }
                return;
            }
            _DicDataType = ConvertFixTagClassConstants(_DataType);
            //2.驗證MQ傳過來的FixData的tag正確性(與指定的DataType)
            foreach (string key in MessageDictionary.Keys)
            {
                if (!_DicDataType.ContainsKey(key))
                {
                    _ErrMsg = string.Format("Fix Data's Tag[{0}] Not in the assigned type[{1}]", key, _DataType.Name);
                    Common.LogHelper.Logger.LogInfo<GenericMQAdapter>(_ErrMsg);
                    if (UISyncContext != null && IsEventInUIThread)
                    {
                        UISyncContext.Post(OnMessageHandleFinished, new MQMessageHandleFinishedEventArgs(_ErrMsg, null));
                    }
                    else
                    {
                        OnMessageHandleFinished(new MQMessageHandleFinishedEventArgs(_ErrMsg, null));
                    }
                    return;
                }
            }

            //建立DataTable Schema
            if (MessageDT.Columns.Count < 1)
            {
                CreateTableSchema(_DicDataType);
            }
            //匯入每筆message到DataTable
            DataRow MessageRow;
            AddMessageToTable(MessageDictionary, out MessageRow);
            if (MessageRow != null && MessageRow.Table.Columns.Contains("MacAddress") && !MessageRow.IsNull("MacAddress") && this.SendName.IndexOf("#") != -1)
            {
                this.ReStartSender(this.SendName.Replace("#", MessageRow["MacAddress"].ToString()));
            }
            //觸發每筆Message資料處理完成事件
            if (UISyncContext != null && IsEventInUIThread)
            {
                UISyncContext.Post(OnMessageHandleFinished, new MQMessageHandleFinishedEventArgs(_ErrMsg, MessageRow));
            }
            else
            {
                OnMessageHandleFinished(new MQMessageHandleFinishedEventArgs(_ErrMsg, MessageRow));
            }
        }

        /// <summary>
        /// 轉換MsgType.cs中Tag Class的常數名稱及常數值為Dictionary
        /// </summary>
        /// <param name="DataType"></param>
        /// <returns></returns>
        protected Dictionary<string, string> ConvertFixTagClassConstants(Type DataType)
        {
            Dictionary<string, string> DicFix = new Dictionary<string, string>();
            FieldInfo[] fieldInfos = DataType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo fi in fieldInfos)
            {
                if (fi.IsLiteral && !fi.IsInitOnly)
                {
                    DicFix.Add(fi.GetValue(DataType).ToString(), fi.Name);
                }
            }
            return DicFix;
        }

        /// <summary>
        /// 建立存放MQMessge資料內容的DataTable Schema
        /// </summary>
        /// <param name="DicDataType">指定的DataType</param>
        private void CreateTableSchema(Dictionary<string, string> DicDataType)
        {
            MessageDT.Reset();
            string MessageID = _DataType.GetField("MessageID") == null ? "710" : _DataType.GetField("MessageID").GetValue(_DataType).ToString();
            string TotalRecords = _DataType.GetField("TotalRecords") == null ? "10038" : _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
            foreach (string key in DicDataType.Keys)
            {
                //必須為非requestID的tag及非總筆數的tag才建立欄位(停用下列程式碼)
                //if (!key.Equals(MessageID) && !key.Equals(TotalRecords))
                //必須為非總筆數的tag才建立欄位
                if (!key.Equals(TotalRecords))
                {
                    string FiledName = DicDataType[key].ToString();
                    MessageDT.Columns.Add(FiledName, typeof(System.String));
                }
            }
        }
        /// <summary>
        /// 將每筆Message加入至DataTable
        /// </summary>
        /// <param name="DicMessage">每筆Message的Dictionary資料形式</param>
        /// <param name="MessagRow"></param>
        private void AddMessageToTable(Dictionary<string, string> DicMessage, out DataRow MessagRow)
        {
            try
            {
                string MessageID = _DataType.GetField("MessageID") == null ? "710" : _DataType.GetField("MessageID").GetValue(_DataType).ToString();
                string TotalRecords = _DataType.GetField("TotalRecords") == null ? "10038" : _DataType.GetField("TotalRecords").GetValue(_DataType).ToString();
                DataRow tmpMessagRow = MessageDT.NewRow();
                foreach (string key in DicMessage.Keys)
                {
                    //必須為非requestID的tag及非總筆數的tag才設定欄位值(停用下列程式碼)
                    //if (!key.Equals(MessageID) && !key.Equals(TotalRecords))
                    //必須為非總筆數的tag才設定欄位值
                    if (!key.Equals(TotalRecords))
                    {
                        tmpMessagRow[_DicDataType[key].ToString()] = DicMessage[key].ToString();
                    }
                }
                MessageDT.Rows.Add(tmpMessagRow);
                MessagRow = tmpMessagRow;
            }
            catch (Exception ex)
            {
                MessagRow = null;
                Common.LogHelper.Logger.LogError<GenericMQAdapter>(ex);
            }
        }
    }
}
