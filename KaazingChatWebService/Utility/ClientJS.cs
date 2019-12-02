using Newtonsoft.Json.Linq;
using System;

namespace KaazingChatWebService.Utility
{
    public class ClientJS
    {
        public static bool IsArrayJson(string Text)
        {
            bool isArrayJson;
            try
            {
                JToken JToken = JToken.Parse(Text);
                if(JToken is JArray)
                {
                    isArrayJson = true;
                }
                else
                {
                    isArrayJson = false;
                }
            }
            catch(Exception)
            {
                isArrayJson = false;
            }
            return isArrayJson;
        }

        public static bool IsObjectJson(string Text)
        {
            bool isObjectJson;
            try
            {
                JToken JToken = JToken.Parse(Text);
                if (JToken is JObject)
                {
                    isObjectJson = true;
                }
                else
                {
                    isObjectJson = false;
                }
            }
            catch (Exception)
            {
                isObjectJson = false;
            }
            return isObjectJson;
        }
    }
}