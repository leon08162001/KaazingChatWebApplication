//using Java.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Utility
{
    public class Util
    {
        [DllImport("Iphlpapi.dll")]
        static extern int SendARP(Int32 DestIP, Int32 SrcIP, ref Int64 MacAddr, ref Int32 PhyAddrLen);
        [DllImport("Ws2_32.dll")]
        static extern Int32 inet_addr(string ipaddr);


        //public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 轉換MsgType.cs中Tag Class的常數名稱及常數值為Dictionary
        /// </summary>
        /// <param name="TagType"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertTagClassConstants(Type TagType)
        {
            Dictionary<string, string> DicTag = new Dictionary<string, string>();
            try
            {
                FieldInfo[] fieldInfos = TagType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                PropertyInfo[] propertyInfos = TagType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                if (fieldInfos.Length > 0)
                {
                    foreach (FieldInfo fi in fieldInfos)
                    {
                        if (fi.IsLiteral && !fi.IsInitOnly)
                        {
                            DicTag.Add(fi.GetValue(TagType).ToString(), fi.Name);
                        }
                    }
                }
                if (propertyInfos.Length > 0)
                {
                    foreach (PropertyInfo property in propertyInfos)
                    {
                        if (property.MemberType == MemberTypes.Property)
                        {
                            DicTag.Add(property.Name, property.Name);
                        }
                    }
                    DicTag.Add("99", "MacAddress");
                    DicTag.Add("710", "MessageID");
                    DicTag.Add("10038", "TotalRecords");
                    DicTag.Add("9999", "Sequence");
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return DicTag;
        }
        /// <summary>
        /// 建立存放MQMessge資料內容的DataTable Schema
        /// </summary>
        /// <param name="DicTagType">指定的TagType</param>
        public static DataTable CreateTableSchema(Dictionary<string, string> DicTagType, Type TagType)
        {
            DataTable MessageDT = new DataTable();
            foreach (string key in DicTagType.Keys)
            {
                string FiledName = DicTagType[key].ToString();
                MessageDT.Columns.Add(FiledName, typeof(System.String));
            }
            return MessageDT;
        }
        /// <summary>
        /// 將每筆MQMessage加入至DataTable
        /// </summary>
        /// <param name="DicMQMessage">每筆MQMessage的Dictionary資料形式</param>
        /// <param name="MessagRow"></param>
        public static DataTable AddMessageToTable(Dictionary<string, string> DicMQMessage, out DataRow MessagRow, Dictionary<string, string> DicTagType, Type TagType, DataTable MessageDT)
        {
            try
            {
                DataRow tmpMessagRow = MessageDT.NewRow();
                foreach (string key in DicMQMessage.Keys)
                {
                    tmpMessagRow[DicTagType[key].ToString()] = DicMQMessage[key] == null ? DicMQMessage[key] : DicMQMessage[key].ToString();
                }
                MessageDT.Rows.Add(tmpMessagRow);
                MessagRow = tmpMessagRow;
            }
            catch (Exception ex)
            {
                MessagRow = null;
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return MessageDT;
        }
        public static DataRow AddMessageToRow(Dictionary<string, string> DicMQMessage, Dictionary<string, string> DicTagType, Type TagType, DataTable MessageDT)
        {
            DataRow tmpMessagRow = MessageDT.NewRow();
            try
            {
                foreach (string key in DicMQMessage.Keys)
                {
                    tmpMessagRow[DicTagType[key].ToString()] = DicMQMessage[key] == null ? DicMQMessage[key] : DicMQMessage[key].ToString();
                }
            }
            catch (Exception ex)
            {
                tmpMessagRow = null;
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return tmpMessagRow;
        }
        /// <summary>
        /// 將字串轉換成Dictionary
        /// </summary>
        /// <param name="Message">字串型態資料</param>
        /// <param name="DataSplitChar">資料分隔字元</param>
        /// <param name="DataMapChar">field和value分隔字元</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToMessageMap(string Message, string DataSplitChar, string DataMapChar)
        {
            Dictionary<string, string> MessageMap = new Dictionary<string, string>();
            IEnumerable<string> IEnumData = Message.Split(DataSplitChar.ToCharArray()).AsEnumerable();
            string TempData = "";
            foreach (string Data in IEnumData)
            {
                if (Data.Contains(DataMapChar))
                {
                    int fixTag;
                    string LeftStr = Data.Substring(0, Data.IndexOf("="));
                    if (int.TryParse(LeftStr, out fixTag))
                    {
                        if (TempData != "")
                        {
                            MessageMap[MessageMap.ElementAt(MessageMap.Count - 1).Key] += TempData;
                            TempData = "";
                        }
                        string[] AryKeyValue = Data.Split(DataMapChar.ToCharArray());
                        if (AryKeyValue.Length == 2 && AryKeyValue[0] != "" && AryKeyValue[1] != "")
                        {
                            MessageMap.Add(AryKeyValue[0], AryKeyValue[1]);
                        }
                        else
                        {
                            TempData = "";
                            for (int i = 1; i < AryKeyValue.Length; i++)
                            {
                                TempData += AryKeyValue[i] + "=";
                            }
                            TempData = TempData.Substring(0, TempData.Length - 1);
                            MessageMap.Add(AryKeyValue[0], TempData);
                            TempData = "";
                        }
                    }
                    else
                    {
                        TempData += " " + Data;
                    }
                }
                else
                {
                    TempData += " " + Data;
                    continue;
                }
            }
            if (TempData != "")
            {
                MessageMap[MessageMap.ElementAt(MessageMap.Count - 1).Key] += TempData;
            }
            return MessageMap;
        }
        /// <summary>
        /// 將Fix字串轉換成Dictionary
        /// </summary>
        /// <param name="Message">字串型態資料</param>
        /// <param name="DataSplitChar">資料分隔字元</param>
        /// <param name="DataMapChar">field和value分隔字元</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToMessageMapForFix(string Message, string DataSplitChar, string DataMapChar)
        {
            Dictionary<string, string> MessageMap = new Dictionary<string, string>();
            try
            {
                IEnumerable<string> IEnumData = Message.Split(DataSplitChar.ToCharArray()).AsEnumerable();
                string TempLostData = "";
                foreach (string Data in IEnumData)
                {
                    if (Data.Contains(DataMapChar))
                    {
                        int fixTag;
                        string LeftStr = Data.Substring(0, Data.IndexOf("="));
                        if (int.TryParse(LeftStr, out fixTag))
                        {
                            if (TempLostData != "")
                            {
                                MessageMap[MessageMap.ElementAt(MessageMap.Count - 1).Key] += TempLostData;
                                TempLostData = "";
                            }
                            string[] AryKeyValue = Data.Split(DataMapChar.ToCharArray());
                            if (AryKeyValue.Length == 2 && AryKeyValue[0] != "" && AryKeyValue[1] != "")
                            {
                                MessageMap.Add(AryKeyValue[0], AryKeyValue[1]);
                            }
                            else
                            {
                                TempLostData = "";
                                for (int i = 1; i < AryKeyValue.Length; i++)
                                {
                                    TempLostData += AryKeyValue[i] + "=";
                                }
                                TempLostData = TempLostData.Substring(0, TempLostData.Length - 1);
                                MessageMap.Add(AryKeyValue[0], TempLostData);
                                TempLostData = "";
                            }
                        }
                        else
                        {
                            TempLostData += " " + Data;
                        }
                    }
                    else
                    {
                        TempLostData += " " + Data;
                        continue;
                    }
                }
                if (TempLostData != "")
                {
                    MessageMap[MessageMap.ElementAt(MessageMap.Count - 1).Key] += TempLostData;
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return MessageMap;
        }

        /// <summary>
        /// 將Json字串轉換成Dictionary
        /// </summary>
        /// <param name="Message">字串型態資料</param>
        /// <param name="DataSplitChar">資料分隔字元</param>
        /// <param name="DataMapChar">field和value分隔字元</param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> ToMessageMapForJson(string sJson)
        {
            List<Dictionary<string, string>> MessageMapList = new List<Dictionary<string, string>>();
            Dictionary<string, string> MessageMap = new Dictionary<string, string>();
            try
            {
                var settings = new JsonSerializerSettings { Converters = new JsonConverter[] { new JsonGenericDictionaryOrArrayConverter() } };
                var json = JValue.Parse(sJson);
                if (json.Type == JTokenType.Object)
                {
                    MessageMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(sJson, settings);
                    MessageMapList.Add(MessageMap);
                }
                else if (json.Type == JTokenType.Array)
                {
                    MessageMapList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(sJson, settings);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return MessageMapList;
        }
        /// <summary>
        /// 取得MacAddress
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <returns></returns>
        /// <summary>
        /// 根據IP取得MacAddress
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static string GetMacAddress(string IPAddress = null)
        {
            string strMacAddress = string.Empty;
            StringBuilder SBMacAddress = new StringBuilder();
            try
            {
                Int32 remote = inet_addr(IPAddress);

                Int64 macinfo = new Int64();
                Int32 length = 6;
                SendARP(remote, 0, ref macinfo, ref length);

                string temp = System.Convert.ToString(macinfo, 16).PadLeft(12, '0').ToUpper();

                int x = 12;
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5) { SBMacAddress.Append(temp.Substring(x - 2, 2)); }
                    else { SBMacAddress.Append(temp.Substring(x - 2, 2) + ":"); }
                    x -= 2;
                }
                strMacAddress = SBMacAddress.ToString();
            }
            catch (Exception ex)
            {
                strMacAddress = string.Empty;
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return strMacAddress;
        }
        public static string GetIosMacAddress()
        {
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    var address = netInterface.GetPhysicalAddress();
                    return BitConverter.ToString(address.GetAddressBytes());

                }
            }

            return "NoMac";
        }
        public static string ConvertToWebServiceFixString(string FixString)
        {
            return FixString.Replace(Convert.ToChar((byte)0x01).ToString(), "|");
        }

        public static T ConvertToType<T>(string fixString) where T : class, new()
        {
            T obj = new T();
            try
            {
                Dictionary<string, string> DicMap = ToMessageMap(fixString, Convert.ToChar((byte)0x01).ToString(), "=");
                Type TagType = typeof(T);
                foreach (string key in DicMap.Keys)
                {
                    string Value = DicMap[key];
                    string Name = GetConstantName<string>(TagType, key);
                    PropertyInfo propertyInfo = obj.GetType().GetProperty(UppercaseFirst(Name));
                    propertyInfo.SetValue(obj, Convert.ChangeType(Value, propertyInfo.PropertyType), null);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return obj;
        }

        public static void AddTypeToList<T>(T obj, ref List<T> TypeList) where T : class, new()
        {
            TypeList.Add(obj);
        }

        public static string GetConstantName<T>(Type containingType, T value)
        {
            try
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                foreach (FieldInfo field in containingType.GetFields
                         (BindingFlags.Static | BindingFlags.Public))
                {
                    if (field.FieldType == typeof(T) &&
                        comparer.Equals(value, (T)field.GetValue(null)))
                    {
                        return field.Name; // There could be others, of course...
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.Logger.LogError<Util>(ex);
            }
            return value.ToString(); // Or throw an exception
        }
        public static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension == null)
                throw new ArgumentNullException("extension");

            if (extension.StartsWith("."))
                extension = extension.Substring(1);


            switch (extension.ToLower())
            {
                #region Big freaking list of mime types
                case "323": return "text/h323";
                case "3g2": return "video/3gpp2";
                case "3gp": return "video/3gpp";
                case "3gp2": return "video/3gpp2";
                case "3gpp": return "video/3gpp";
                case "7z": return "application/x-7z-compressed";
                case "aa": return "audio/audible";
                case "aac": return "audio/aac";
                case "aaf": return "application/octet-stream";
                case "aax": return "audio/vnd.audible.aax";
                case "ac3": return "audio/ac3";
                case "aca": return "application/octet-stream";
                case "accda": return "application/msaccess.addin";
                case "accdb": return "application/msaccess";
                case "accdc": return "application/msaccess.cab";
                case "accde": return "application/msaccess";
                case "accdr": return "application/msaccess.runtime";
                case "accdt": return "application/msaccess";
                case "accdw": return "application/msaccess.webapplication";
                case "accft": return "application/msaccess.ftemplate";
                case "acx": return "application/internet-property-stream";
                case "addin": return "text/xml";
                case "ade": return "application/msaccess";
                case "adobebridge": return "application/x-bridge-url";
                case "adp": return "application/msaccess";
                case "adt": return "audio/vnd.dlna.adts";
                case "adts": return "audio/aac";
                case "afm": return "application/octet-stream";
                case "ai": return "application/postscript";
                case "aif": return "audio/x-aiff";
                case "aifc": return "audio/aiff";
                case "aiff": return "audio/aiff";
                case "air": return "application/vnd.adobe.air-application-installer-package+zip";
                case "amc": return "application/x-mpeg";
                case "application": return "application/x-ms-application";
                case "art": return "image/x-jg";
                case "asa": return "application/xml";
                case "asax": return "application/xml";
                case "ascx": return "application/xml";
                case "asd": return "application/octet-stream";
                case "asf": return "video/x-ms-asf";
                case "ashx": return "application/xml";
                case "asi": return "application/octet-stream";
                case "asm": return "text/plain";
                case "asmx": return "application/xml";
                case "aspx": return "application/xml";
                case "asr": return "video/x-ms-asf";
                case "asx": return "video/x-ms-asf";
                case "atom": return "application/atom+xml";
                case "au": return "audio/basic";
                case "avi": return "video/x-msvideo";
                case "axs": return "application/olescript";
                case "bas": return "text/plain";
                case "bcpio": return "application/x-bcpio";
                case "bin": return "application/octet-stream";
                case "bmp": return "image/bmp";
                case "c": return "text/plain";
                case "cab": return "application/octet-stream";
                case "caf": return "audio/x-caf";
                case "calx": return "application/vnd.ms-office.calx";
                case "cat": return "application/vnd.ms-pki.seccat";
                case "cc": return "text/plain";
                case "cd": return "text/plain";
                case "cdda": return "audio/aiff";
                case "cdf": return "application/x-cdf";
                case "cer": return "application/x-x509-ca-cert";
                case "chm": return "application/octet-stream";
                case "class": return "application/x-java-applet";
                case "clp": return "application/x-msclip";
                case "cmx": return "image/x-cmx";
                case "cnf": return "text/plain";
                case "cod": return "image/cis-cod";
                case "config": return "application/xml";
                case "contact": return "text/x-ms-contact";
                case "coverage": return "application/xml";
                case "cpio": return "application/x-cpio";
                case "cpp": return "text/plain";
                case "crd": return "application/x-mscardfile";
                case "crl": return "application/pkix-crl";
                case "crt": return "application/x-x509-ca-cert";
                case "cs": return "text/plain";
                case "csdproj": return "text/plain";
                case "csh": return "application/x-csh";
                case "csproj": return "text/plain";
                case "css": return "text/css";
                case "csv": return "text/csv";
                case "cur": return "application/octet-stream";
                case "cxx": return "text/plain";
                case "dat": return "application/octet-stream";
                case "datasource": return "application/xml";
                case "dbproj": return "text/plain";
                case "dcr": return "application/x-director";
                case "def": return "text/plain";
                case "deploy": return "application/octet-stream";
                case "der": return "application/x-x509-ca-cert";
                case "dgml": return "application/xml";
                case "dib": return "image/bmp";
                case "dif": return "video/x-dv";
                case "dir": return "application/x-director";
                case "disco": return "text/xml";
                case "dll": return "application/x-msdownload";
                case "dll.config": return "text/xml";
                case "dlm": return "text/dlm";
                case "doc": return "application/msword";
                case "docm": return "application/vnd.ms-word.document.macroenabled.12";
                case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "dot": return "application/msword";
                case "dotm": return "application/vnd.ms-word.template.macroenabled.12";
                case "dotx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                case "dsp": return "application/octet-stream";
                case "dsw": return "text/plain";
                case "dtd": return "text/xml";
                case "dtsconfig": return "text/xml";
                case "dv": return "video/x-dv";
                case "dvi": return "application/x-dvi";
                case "dwf": return "drawing/x-dwf";
                case "dwp": return "application/octet-stream";
                case "dxr": return "application/x-director";
                case "eml": return "message/rfc822";
                case "emz": return "application/octet-stream";
                case "eot": return "application/octet-stream";
                case "eps": return "application/postscript";
                case "etl": return "application/etl";
                case "etx": return "text/x-setext";
                case "evy": return "application/envoy";
                case "exe": return "application/octet-stream";
                case "exe.config": return "text/xml";
                case "fdf": return "application/vnd.fdf";
                case "fif": return "application/fractals";
                case "filters": return "application/xml";
                case "fla": return "application/octet-stream";
                case "flr": return "x-world/x-vrml";
                case "flv": return "video/x-flv";
                case "fsscript": return "application/fsharp-script";
                case "fsx": return "application/fsharp-script";
                case "generictest": return "application/xml";
                case "gif": return "image/gif";
                case "group": return "text/x-ms-group";
                case "gsm": return "audio/x-gsm";
                case "gtar": return "application/x-gtar";
                case "gz": return "application/x-gzip";
                case "h": return "text/plain";
                case "hdf": return "application/x-hdf";
                case "hdml": return "text/x-hdml";
                case "hhc": return "application/x-oleobject";
                case "hhk": return "application/octet-stream";
                case "hhp": return "application/octet-stream";
                case "hlp": return "application/winhlp";
                case "hpp": return "text/plain";
                case "hqx": return "application/mac-binhex40";
                case "hta": return "application/hta";
                case "htc": return "text/x-component";
                case "htm": return "text/html";
                case "html": return "text/html";
                case "htt": return "text/webviewhtml";
                case "hxa": return "application/xml";
                case "hxc": return "application/xml";
                case "hxd": return "application/octet-stream";
                case "hxe": return "application/xml";
                case "hxf": return "application/xml";
                case "hxh": return "application/octet-stream";
                case "hxi": return "application/octet-stream";
                case "hxk": return "application/xml";
                case "hxq": return "application/octet-stream";
                case "hxr": return "application/octet-stream";
                case "hxs": return "application/octet-stream";
                case "hxt": return "text/html";
                case "hxv": return "application/xml";
                case "hxw": return "application/octet-stream";
                case "hxx": return "text/plain";
                case "i": return "text/plain";
                case "ico": return "image/x-icon";
                case "ics": return "application/octet-stream";
                case "idl": return "text/plain";
                case "ief": return "image/ief";
                case "iii": return "application/x-iphone";
                case "inc": return "text/plain";
                case "inf": return "application/octet-stream";
                case "inl": return "text/plain";
                case "ins": return "application/x-internet-signup";
                case "ipa": return "application/x-itunes-ipa";
                case "ipg": return "application/x-itunes-ipg";
                case "ipproj": return "text/plain";
                case "ipsw": return "application/x-itunes-ipsw";
                case "iqy": return "text/x-ms-iqy";
                case "isp": return "application/x-internet-signup";
                case "ite": return "application/x-itunes-ite";
                case "itlp": return "application/x-itunes-itlp";
                case "itms": return "application/x-itunes-itms";
                case "itpc": return "application/x-itunes-itpc";
                case "ivf": return "video/x-ivf";
                case "jar": return "application/java-archive";
                case "java": return "application/octet-stream";
                case "jck": return "application/liquidmotion";
                case "jcz": return "application/liquidmotion";
                case "jfif": return "image/pjpeg";
                case "jnlp": return "application/x-java-jnlp-file";
                case "jpb": return "application/octet-stream";
                case "jpe": return "image/jpeg";
                case "jpeg": return "image/jpeg";
                case "jpg": return "image/jpeg";
                case "js": return "application/x-javascript";
                case "jsx": return "text/jscript";
                case "jsxbin": return "text/plain";
                case "latex": return "application/x-latex";
                case "library-ms": return "application/windows-library+xml";
                case "lit": return "application/x-ms-reader";
                case "loadtest": return "application/xml";
                case "lpk": return "application/octet-stream";
                case "lsf": return "video/x-la-asf";
                case "lst": return "text/plain";
                case "lsx": return "video/x-la-asf";
                case "lzh": return "application/octet-stream";
                case "m13": return "application/x-msmediaview";
                case "m14": return "application/x-msmediaview";
                case "m1v": return "video/mpeg";
                case "m2t": return "video/vnd.dlna.mpeg-tts";
                case "m2ts": return "video/vnd.dlna.mpeg-tts";
                case "m2v": return "video/mpeg";
                case "m3u": return "audio/x-mpegurl";
                case "m3u8": return "audio/x-mpegurl";
                case "m4a": return "audio/m4a";
                case "m4b": return "audio/m4b";
                case "m4p": return "audio/m4p";
                case "m4r": return "audio/x-m4r";
                case "m4v": return "video/x-m4v";
                case "mac": return "image/x-macpaint";
                case "mak": return "text/plain";
                case "man": return "application/x-troff-man";
                case "manifest": return "application/x-ms-manifest";
                case "map": return "text/plain";
                case "master": return "application/xml";
                case "mda": return "application/msaccess";
                case "mdb": return "application/x-msaccess";
                case "mde": return "application/msaccess";
                case "mdp": return "application/octet-stream";
                case "me": return "application/x-troff-me";
                case "mfp": return "application/x-shockwave-flash";
                case "mht": return "message/rfc822";
                case "mhtml": return "message/rfc822";
                case "mid": return "audio/mid";
                case "midi": return "audio/mid";
                case "mix": return "application/octet-stream";
                case "mk": return "text/plain";
                case "mmf": return "application/x-smaf";
                case "mno": return "text/xml";
                case "mny": return "application/x-msmoney";
                case "mod": return "video/mpeg";
                case "mov": return "video/quicktime";
                case "movie": return "video/x-sgi-movie";
                case "mp2": return "video/mpeg";
                case "mp2v": return "video/mpeg";
                case "mp3": return "audio/mpeg";
                case "mp4": return "video/mp4";
                case "mp4v": return "video/mp4";
                case "mpa": return "video/mpeg";
                case "mpe": return "video/mpeg";
                case "mpeg": return "video/mpeg";
                case "mpf": return "application/vnd.ms-mediapackage";
                case "mpg": return "video/mpeg";
                case "mpp": return "application/vnd.ms-project";
                case "mpv2": return "video/mpeg";
                case "mqv": return "video/quicktime";
                case "ms": return "application/x-troff-ms";
                case "msi": return "application/octet-stream";
                case "mso": return "application/octet-stream";
                case "mts": return "video/vnd.dlna.mpeg-tts";
                case "mtx": return "application/xml";
                case "mvb": return "application/x-msmediaview";
                case "mvc": return "application/x-miva-compiled";
                case "mxp": return "application/x-mmxp";
                case "nc": return "application/x-netcdf";
                case "nsc": return "video/x-ms-asf";
                case "nws": return "message/rfc822";
                case "ocx": return "application/octet-stream";
                case "oda": return "application/oda";
                case "odc": return "text/x-ms-odc";
                case "odh": return "text/plain";
                case "odl": return "text/plain";
                case "odp": return "application/vnd.oasis.opendocument.presentation";
                case "ods": return "application/oleobject";
                case "odt": return "application/vnd.oasis.opendocument.text";
                case "ogg": return "video/ogg";
                case "ogv": return "video/ogg";
                case "one": return "application/onenote";
                case "onea": return "application/onenote";
                case "onepkg": return "application/onenote";
                case "onetmp": return "application/onenote";
                case "onetoc": return "application/onenote";
                case "onetoc2": return "application/onenote";
                case "orderedtest": return "application/xml";
                case "osdx": return "application/opensearchdescription+xml";
                case "p10": return "application/pkcs10";
                case "p12": return "application/x-pkcs12";
                case "p7b": return "application/x-pkcs7-certificates";
                case "p7c": return "application/pkcs7-mime";
                case "p7m": return "application/pkcs7-mime";
                case "p7r": return "application/x-pkcs7-certreqresp";
                case "p7s": return "application/pkcs7-signature";
                case "pbm": return "image/x-portable-bitmap";
                case "pcast": return "application/x-podcast";
                case "pct": return "image/pict";
                case "pcx": return "application/octet-stream";
                case "pcz": return "application/octet-stream";
                case "pdf": return "application/pdf";
                case "pfb": return "application/octet-stream";
                case "pfm": return "application/octet-stream";
                case "pfx": return "application/x-pkcs12";
                case "pgm": return "image/x-portable-graymap";
                case "pic": return "image/pict";
                case "pict": return "image/pict";
                case "pkgdef": return "text/plain";
                case "pkgundef": return "text/plain";
                case "pko": return "application/vnd.ms-pki.pko";
                case "pls": return "audio/scpls";
                case "pma": return "application/x-perfmon";
                case "pmc": return "application/x-perfmon";
                case "pml": return "application/x-perfmon";
                case "pmr": return "application/x-perfmon";
                case "pmw": return "application/x-perfmon";
                case "png": return "image/png";
                case "pnm": return "image/x-portable-anymap";
                case "pnt": return "image/x-macpaint";
                case "pntg": return "image/x-macpaint";
                case "pnz": return "image/png";
                case "pot": return "application/vnd.ms-powerpoint";
                case "potm": return "application/vnd.ms-powerpoint.template.macroenabled.12";
                case "potx": return "application/vnd.openxmlformats-officedocument.presentationml.template";
                case "ppa": return "application/vnd.ms-powerpoint";
                case "ppam": return "application/vnd.ms-powerpoint.addin.macroenabled.12";
                case "ppm": return "image/x-portable-pixmap";
                case "pps": return "application/vnd.ms-powerpoint";
                case "ppsm": return "application/vnd.ms-powerpoint.slideshow.macroenabled.12";
                case "ppsx": return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                case "ppt": return "application/vnd.ms-powerpoint";
                case "pptm": return "application/vnd.ms-powerpoint.presentation.macroenabled.12";
                case "pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "prf": return "application/pics-rules";
                case "prm": return "application/octet-stream";
                case "prx": return "application/octet-stream";
                case "ps": return "application/postscript";
                case "psc1": return "application/powershell";
                case "psd": return "application/octet-stream";
                case "psess": return "application/xml";
                case "psm": return "application/octet-stream";
                case "psp": return "application/octet-stream";
                case "pub": return "application/x-mspublisher";
                case "pwz": return "application/vnd.ms-powerpoint";
                case "qht": return "text/x-html-insertion";
                case "qhtm": return "text/x-html-insertion";
                case "qt": return "video/quicktime";
                case "qti": return "image/x-quicktime";
                case "qtif": return "image/x-quicktime";
                case "qtl": return "application/x-quicktimeplayer";
                case "qxd": return "application/octet-stream";
                case "ra": return "audio/x-pn-realaudio";
                case "ram": return "audio/x-pn-realaudio";
                case "rar": return "application/octet-stream";
                case "ras": return "image/x-cmu-raster";
                case "rat": return "application/rat-file";
                case "rc": return "text/plain";
                case "rc2": return "text/plain";
                case "rct": return "text/plain";
                case "rdlc": return "application/xml";
                case "resx": return "application/xml";
                case "rf": return "image/vnd.rn-realflash";
                case "rgb": return "image/x-rgb";
                case "rgs": return "text/plain";
                case "rm": return "application/vnd.rn-realmedia";
                case "rmi": return "audio/mid";
                case "rmp": return "application/vnd.rn-rn_music_package";
                case "roff": return "application/x-troff";
                case "rpm": return "audio/x-pn-realaudio-plugin";
                case "rqy": return "text/x-ms-rqy";
                case "rtf": return "application/rtf";
                case "rtx": return "text/richtext";
                case "ruleset": return "application/xml";
                case "s": return "text/plain";
                case "safariextz": return "application/x-safari-safariextz";
                case "scd": return "application/x-msschedule";
                case "sct": return "text/scriptlet";
                case "sd2": return "audio/x-sd2";
                case "sdp": return "application/sdp";
                case "sea": return "application/octet-stream";
                case "searchconnector-ms": return "application/windows-search-connector+xml";
                case "setpay": return "application/set-payment-initiation";
                case "setreg": return "application/set-registration-initiation";
                case "settings": return "application/xml";
                case "sgimb": return "application/x-sgimb";
                case "sgml": return "text/sgml";
                case "sh": return "application/x-sh";
                case "shar": return "application/x-shar";
                case "shtml": return "text/html";
                case "sit": return "application/x-stuffit";
                case "sitemap": return "application/xml";
                case "skin": return "application/xml";
                case "sldm": return "application/vnd.ms-powerpoint.slide.macroenabled.12";
                case "sldx": return "application/vnd.openxmlformats-officedocument.presentationml.slide";
                case "slk": return "application/vnd.ms-excel";
                case "sln": return "text/plain";
                case "slupkg-ms": return "application/x-ms-license";
                case "smd": return "audio/x-smd";
                case "smi": return "application/octet-stream";
                case "smx": return "audio/x-smd";
                case "smz": return "audio/x-smd";
                case "snd": return "audio/basic";
                case "snippet": return "application/xml";
                case "snp": return "application/octet-stream";
                case "sol": return "text/plain";
                case "sor": return "text/plain";
                case "spc": return "application/x-pkcs7-certificates";
                case "spl": return "application/futuresplash";
                case "src": return "application/x-wais-source";
                case "srf": return "text/plain";
                case "ssisdeploymentmanifest": return "text/xml";
                case "ssm": return "application/streamingmedia";
                case "sst": return "application/vnd.ms-pki.certstore";
                case "stl": return "application/vnd.ms-pki.stl";
                case "sv4cpio": return "application/x-sv4cpio";
                case "sv4crc": return "application/x-sv4crc";
                case "svc": return "application/xml";
                case "swf": return "application/x-shockwave-flash";
                case "t": return "application/x-troff";
                case "tar": return "application/x-tar";
                case "tcl": return "application/x-tcl";
                case "testrunconfig": return "application/xml";
                case "testsettings": return "application/xml";
                case "tex": return "application/x-tex";
                case "texi": return "application/x-texinfo";
                case "texinfo": return "application/x-texinfo";
                case "tgz": return "application/x-compressed";
                case "thmx": return "application/vnd.ms-officetheme";
                case "thn": return "application/octet-stream";
                case "tif": return "image/tiff";
                case "tiff": return "image/tiff";
                case "tlh": return "text/plain";
                case "tli": return "text/plain";
                case "toc": return "application/octet-stream";
                case "tr": return "application/x-troff";
                case "trm": return "application/x-msterminal";
                case "trx": return "application/xml";
                case "ts": return "video/vnd.dlna.mpeg-tts";
                case "tsv": return "text/tab-separated-values";
                case "ttf": return "application/octet-stream";
                case "tts": return "video/vnd.dlna.mpeg-tts";
                case "txt": return "text/plain";
                case "u32": return "application/octet-stream";
                case "uls": return "text/iuls";
                case "user": return "text/plain";
                case "ustar": return "application/x-ustar";
                case "vb": return "text/plain";
                case "vbdproj": return "text/plain";
                case "vbk": return "video/mpeg";
                case "vbproj": return "text/plain";
                case "vbs": return "text/vbscript";
                case "vcf": return "text/x-vcard";
                case "vcproj": return "application/xml";
                case "vcs": return "text/plain";
                case "vcxproj": return "application/xml";
                case "vddproj": return "text/plain";
                case "vdp": return "text/plain";
                case "vdproj": return "text/plain";
                case "vdx": return "application/vnd.ms-visio.viewer";
                case "vml": return "text/xml";
                case "vscontent": return "application/xml";
                case "vsct": return "text/xml";
                case "vsd": return "application/vnd.visio";
                case "vsi": return "application/ms-vsi";
                case "vsix": return "application/vsix";
                case "vsixlangpack": return "text/xml";
                case "vsixmanifest": return "text/xml";
                case "vsmdi": return "application/xml";
                case "vspscc": return "text/plain";
                case "vss": return "application/vnd.visio";
                case "vsscc": return "text/plain";
                case "vssettings": return "text/xml";
                case "vssscc": return "text/plain";
                case "vst": return "application/vnd.visio";
                case "vstemplate": return "text/xml";
                case "vsto": return "application/x-ms-vsto";
                case "vsw": return "application/vnd.visio";
                case "vsx": return "application/vnd.visio";
                case "vtx": return "application/vnd.visio";
                case "wav": return "audio/wav";
                case "wave": return "audio/wav";
                case "wax": return "audio/x-ms-wax";
                case "wbk": return "application/msword";
                case "wbmp": return "image/vnd.wap.wbmp";
                case "wcm": return "application/vnd.ms-works";
                case "wdb": return "application/vnd.ms-works";
                case "wdp": return "image/vnd.ms-photo";
                case "webarchive": return "application/x-safari-webarchive";
                case "webm": return "video/webm";
                case "webtest": return "application/xml";
                case "wiq": return "application/xml";
                case "wiz": return "application/msword";
                case "wks": return "application/vnd.ms-works";
                case "wlmp": return "application/wlmoviemaker";
                case "wlpginstall": return "application/x-wlpg-detect";
                case "wlpginstall3": return "application/x-wlpg3-detect";
                case "wm": return "video/x-ms-wm";
                case "wma": return "audio/x-ms-wma";
                case "wmd": return "application/x-ms-wmd";
                case "wmf": return "application/x-msmetafile";
                case "wml": return "text/vnd.wap.wml";
                case "wmlc": return "application/vnd.wap.wmlc";
                case "wmls": return "text/vnd.wap.wmlscript";
                case "wmlsc": return "application/vnd.wap.wmlscriptc";
                case "wmp": return "video/x-ms-wmp";
                case "wmv": return "video/x-ms-wmv";
                case "wmx": return "video/x-ms-wmx";
                case "wmz": return "application/x-ms-wmz";
                case "wpl": return "application/vnd.ms-wpl";
                case "wps": return "application/vnd.ms-works";
                case "wri": return "application/x-mswrite";
                case "wrl": return "x-world/x-vrml";
                case "wrz": return "x-world/x-vrml";
                case "wsc": return "text/scriptlet";
                case "wsdl": return "text/xml";
                case "wvx": return "video/x-ms-wvx";
                case "x": return "application/directx";
                case "xaf": return "x-world/x-vrml";
                case "xaml": return "application/xaml+xml";
                case "xap": return "application/x-silverlight-app";
                case "xbap": return "application/x-ms-xbap";
                case "xbm": return "image/x-xbitmap";
                case "xdr": return "text/plain";
                case "xht": return "application/xhtml+xml";
                case "xhtml": return "application/xhtml+xml";
                case "xla": return "application/vnd.ms-excel";
                case "xlam": return "application/vnd.ms-excel.addin.macroenabled.12";
                case "xlc": return "application/vnd.ms-excel";
                case "xld": return "application/vnd.ms-excel";
                case "xlk": return "application/vnd.ms-excel";
                case "xll": return "application/vnd.ms-excel";
                case "xlm": return "application/vnd.ms-excel";
                case "xls": return "application/vnd.ms-excel";
                case "xlsb": return "application/vnd.ms-excel.sheet.binary.macroenabled.12";
                case "xlsm": return "application/vnd.ms-excel.sheet.macroenabled.12";
                case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xlt": return "application/vnd.ms-excel";
                case "xltm": return "application/vnd.ms-excel.template.macroenabled.12";
                case "xltx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                case "xlw": return "application/vnd.ms-excel";
                case "xml": return "text/xml";
                case "xmta": return "application/xml";
                case "xof": return "x-world/x-vrml";
                case "xoml": return "text/plain";
                case "xpm": return "image/x-xpixmap";
                case "xps": return "application/vnd.ms-xpsdocument";
                case "xrm-ms": return "text/xml";
                case "xsc": return "application/xml";
                case "xsd": return "text/xml";
                case "xsf": return "text/xml";
                case "xsl": return "text/xml";
                case "xslt": return "text/xml";
                case "xsn": return "application/octet-stream";
                case "xss": return "application/xml";
                case "xtp": return "application/octet-stream";
                case "xwd": return "image/x-xwindowdump";
                case "z": return "application/x-compress";
                case "zip": return "application/x-zip-compressed";
                #endregion
                default: return "application/octet-stream";
            }
        }
        private static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
        public static string ConvertToGenericFixString(string WebServiceFixString)
        {
            return WebServiceFixString.Replace("|", Convert.ToChar((byte)0x01).ToString());
        }
        public static string GetEMSFailOverConnString(string Urls, string Ports, bool useSSL = false)
        {
            StringBuilder FailOverConnString = new StringBuilder("");
            //代表只有1個IP
            if (Urls.IndexOf(",") == -1)
            {
                string port;
                //代表只有1個port
                if (Ports.IndexOf(",") == -1)
                {
                    port = Ports;
                    if (useSSL)
                    {
                        FailOverConnString.Append("ssl://" + Urls + ":" + port);
                    }
                    else
                    {
                        FailOverConnString.Append("tcp://" + Urls + ":" + port);
                    }
                }
                //代表多個port
                else
                {
                    int portsCount = Ports.Split(new char[] { ',' }).Length;
                    for (int i = 0; i < portsCount; i++)
                    {
                        if (useSSL)
                        {
                            FailOverConnString.Append("ssl://" + Urls + ":" + Ports.Split(new char[] { ',' })[i]);
                            FailOverConnString.Append(",");
                        }
                        else
                        {
                            FailOverConnString.Append("tcp://" + Urls + ":" + Ports.Split(new char[] { ',' })[i] + ",");
                        }
                    }
                    if (FailOverConnString.Length > 0)
                    {
                        FailOverConnString.Remove(FailOverConnString.Length - 1, 1);
                    }
                }
            }
            //代表多個IP
            else
            {
                string sPort = "";
                List<string> urls = Urls.Split(new char[] { ',' }).ToList<string>();
                int i = 0;
                foreach (string url in urls)
                {
                    //代表只有1個port
                    if (Ports.IndexOf(",") == -1)
                    {
                        sPort = string.IsNullOrEmpty(Ports) ? "7222" : Ports;
                    }
                    else
                    {
                        sPort = Ports.Split(new char[] { ',' })[i];
                        i++;
                    }
                    if (useSSL)
                    {
                        FailOverConnString.Append("ssl://" + url + ":" + sPort);
                        FailOverConnString.Append(",");
                    }
                    else
                    {
                        FailOverConnString.Append("tcp://" + url + ":" + sPort + ",");
                    }
                }
                if (FailOverConnString.Length > 0)
                {
                    FailOverConnString.Remove(FailOverConnString.Length - 1, 1);
                }
            }
            return FailOverConnString.ToString();
        }

        public static string GetMQFailOverConnString(string Urls, string Ports, bool useSSL = false)
        {
            //string OtherFailOverOptions = "?transport.randomize=true&amp;transport.trackMessages=true&amp;transport.priorityBackup=true";
            string OtherFailOverOptions = "?randomize=false&trackMessages=true&priorityBackup=true&compressLargeMessages=true";
            StringBuilder FailOverConnString = new StringBuilder("");
            //代表只有1個IP
            if (Urls.IndexOf(",") == -1)
            {
                string port;
                //代表只有1個port
                if (Ports.IndexOf(",") == -1)
                {
                    port = Ports;
                    if (useSSL)
                    {
                        FailOverConnString.Append("failover:ssl://" + Urls + ":" + port);
                        FailOverConnString.Append("?transport.acceptInvalidBrokerCert=false");
                        //FailOverConnString.Append("&amp;transport.clientCertFilename=amq-client.ts");
                        //FailOverConnString.Append("&amp;transport.clientCertPassword=880816");
                        //FailOverConnString.Append("&amp;transport.serverName=60.248.159.60");
                        FailOverConnString.Append("&amp;transport.needClientAuth=false");
                    }
                    else
                    {
                        FailOverConnString.Append("failover:tcp://" + Urls + ":" + port);
                    }
                }
                //代表多個port
                else
                {
                    int portsCount = Ports.Split(new char[] { ',' }).Length;
                    for (int i = 0; i < portsCount; i++)
                    {
                        if (useSSL)
                        {
                            FailOverConnString.Append("ssl://" + Urls + ":" + Ports.Split(new char[] { ',' })[i]);
                            FailOverConnString.Append("?transport.acceptInvalidBrokerCert=false");
                            FailOverConnString.Append("&amp;transport.needClientAuth=false");
                            FailOverConnString.Append(",");
                        }
                        else
                        {
                            FailOverConnString.Append("tcp://" + Urls + ":" + Ports.Split(new char[] { ',' })[i] + ",");
                        }
                    }
                    if (FailOverConnString.Length > 0)
                    {
                        FailOverConnString.Insert(0, "failover:(");
                        FailOverConnString.Remove(FailOverConnString.Length - 1, 1);
                        FailOverConnString.Append(")");
                        FailOverConnString.Append(OtherFailOverOptions);
                    }
                }
            }
            //代表多個IP
            else
            {
                string sPort = "";
                List<string> urls = Urls.Split(new char[] { ',' }).ToList<string>();
                int i = 0;
                foreach (string url in urls)
                {
                    //代表只有1個port
                    if (Ports.IndexOf(",") == -1)
                    {
                        sPort = string.IsNullOrEmpty(Ports) ? "61616" : Ports;
                    }
                    else
                    {
                        sPort = Ports.Split(new char[] { ',' })[i];
                        i++;
                    }
                    if (useSSL)
                    {
                        FailOverConnString.Append("ssl://" + url + ":" + sPort);
                        FailOverConnString.Append("?transport.acceptInvalidBrokerCert=false");
                        FailOverConnString.Append("&amp;transport.needClientAuth=false");
                        FailOverConnString.Append(",");
                    }
                    else
                    {
                        FailOverConnString.Append("tcp://" + url + ":" + sPort + ",");
                    }
                }
                if (FailOverConnString.Length > 0)
                {
                    FailOverConnString.Insert(0, "failover:(");
                    FailOverConnString.Remove(FailOverConnString.Length - 1, 1);
                    FailOverConnString.Append(")");
                    FailOverConnString.Append(OtherFailOverOptions);
                }
            }
            return FailOverConnString.ToString();
        }
        public static List<T> DataTableToList<T>(DataTable DT) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (DataRow row in DT.Rows)
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }
    }
}
