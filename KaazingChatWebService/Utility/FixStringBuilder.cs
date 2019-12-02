using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KaazingChatWebService.Utility
{
    public static class FixStringBuilder
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string ConvertToFixString<T>(T InstanceType)
        {
            StringBuilder sb = new StringBuilder("");
            try
            {
                Dictionary<string, string> InstanceTypeDic = new Dictionary<string, string>();
                PropertyInfo[] Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo Property in Properties)
                {
                    object value = InstanceType.GetType().GetProperty(Property.Name).GetValue(InstanceType);
                    if (value != null)
                    {
                        InstanceTypeDic.Add(Property.Name.ToLower(), value.ToString());
                    }
                }
                foreach (var f in typeof(T).GetFields())
                {
                    if (f.IsLiteral)
                    {
                        if (InstanceTypeDic.ContainsKey(f.Name.ToLower()))
                        {
                            sb.AppendFormat("{0}={1}", f.GetValue(InstanceType).ToString(), InstanceTypeDic[f.Name.ToLower()]);
                            sb.AppendFormat("{0}", ((char)1).ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
            }
            return sb.ToString();
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
            return MessageMap;
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
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
            }
            return obj;
        }

        public static void AddTypeToList<T>(T obj,ref List<T> TypeList) where T : class, new()
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
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
            }
            return value.ToString(); // Or throw an exception
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
    }
}