using Common.Utility;
using System;
using System.IO;

namespace Common
{
    public static class ConfigIOS
    {
        public static Stream ConfigStream = null;

        //ActiveMQ&ApolloMQ Setting
        public static string ApolloMQUserID;
        public static string ApolloMQPwd;
        public static string ApolloMQ_service;
        public static string ApolloMQ_network;
        public static string MQReceivedMessageReservedSeconds = "20";

        //EMS Setting
        public static string EMSUserID;
        public static string EMSPwd;
        public static string EMS_service;
        public static string EMS_network;
        public static string EMSReceivedMessageReservedSeconds = "30";

        //DBWebService
        public static string dbWebService;
        public static int webServiceTimeOut = 60;

        //LogFolder
        public static string logDir = "";
        public static int preservedDaysForLog = 10;

        //PreservedRowsOfMessges
        public static int preservedRowsForMessage = 200;

        ////Y77
        public static string jefferiesExcuReport_Sender_Topic;
        public static string jefferiesExcuReport_Listener_Topic;
        public static string jefferiesExcuReportMaxThreads = "0";
        public static bool isUsingThreadPoolThreadForY77 = true;

        public static void ReadParameter()
        {
            if (ConfigStream == null)
            {
                Common.LogHelper.Logger.LogInfo("Config.cs: not yet be assigned config");
                return;
            }
            using (StreamReader sr = new StreamReader(ConfigStream))
            {
                try
                {
                    while (sr.Peek() > 0)
                    {
                        string line = sr.ReadLine().Trim();

                        if (line == "")
                            continue;

                        if (line[0] == '#')
                            continue;

                        int seperator = line.IndexOf("=");

                        if (seperator <= 0)
                            return;

                        string config_name = line.Substring(0, seperator);
                        string config_value = line.Substring(seperator + 1, line.Length - seperator - 1)
                            .Replace("#", string.IsNullOrEmpty(Util.GetIosMacAddress()) ? "#" : Util.GetIosMacAddress());
                        //string config_value = line.Substring(seperator + 1, line.Length - seperator - 1);
                        config_name = config_name.ToUpper();

                        switch (config_name)
                        {
                            //ActiveMQ&ApolloMQ Setting
                            case "APOLLOMQUSERID":
                                ApolloMQUserID = config_value;
                                break;
                            case "APOLLOMQPWD":
                                ApolloMQPwd = config_value;
                                break;
                            case "APOLLOMQ_SERVICE":
                                ApolloMQ_service = config_value;
                                break;
                            case "APOLLOMQ_NETWORK":
                                ApolloMQ_network = config_value;
                                break;
                            case "MQRECEIVEDMESSAGERESERVEDSECONDS":
                                {
                                    int TestValue;
                                    MQReceivedMessageReservedSeconds = int.TryParse(config_value, out TestValue)
                                        ? TestValue.ToString()
                                        : MQReceivedMessageReservedSeconds;
                                    break;
                                }

                            //EMS Setting
                            case "EMSUSERID":
                                EMSUserID = config_value;
                                break;
                            case "EMSPWD":
                                EMSPwd = config_value;
                                break;
                            case "EMS_SERVICE":
                                EMS_service = config_value;
                                break;
                            case "EMS_NETWORK":
                                EMS_network = config_value;
                                break;
                            case "EMSRECEIVEDMESSAGERESERVEDSECONDS":
                                {
                                    int TestValue;
                                    EMSReceivedMessageReservedSeconds = int.TryParse(config_value, out TestValue)
                                        ? TestValue.ToString()
                                        : EMSReceivedMessageReservedSeconds;
                                    break;
                                }

                            //DBWebService
                            case "DBWEBSERVICE":
                                dbWebService = config_value;
                                break;
                            case "WEBSERVICETIMEOUT":
                                {
                                    int ConfigValue;
                                    webServiceTimeOut = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : webServiceTimeOut;
                                    break;
                                }

                            //LogFolder
                            case "LOGDIR":
                                logDir = config_value;
                                break;
                            case "PRESERVEDDAYSFORLOG":
                                {
                                    int ConfigValue;
                                    preservedDaysForLog = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : preservedDaysForLog;
                                    break;
                                }

                            //PreservedRowsOfMessges
                            case "PRESERVEDROWSFORMESSAGE":
                                {
                                    int ConfigValue;
                                    preservedRowsForMessage = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : preservedRowsForMessage;
                                    break;
                                }

                            //Y77
                            case "JEFFERIESEXCUREPORT_SENDER_TOPIC":
                                jefferiesExcuReport_Sender_Topic = config_value;
                                break;
                            case "JEFFERIESEXCUREPORT_LISTENER_TOPIC":
                                jefferiesExcuReport_Listener_Topic = config_value;
                                break;
                            case "JEFFERIESEXCUREPORTMAXTHREADS":
                                {
                                    int ConfigValue;
                                    jefferiesExcuReportMaxThreads = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue.ToString()
                                        : jefferiesExcuReportMaxThreads;
                                    break;
                                }
                            case "ISUSINGTHREADPOOLTHREADFORY77":
                                {
                                    bool ConfigValue;
                                    isUsingThreadPoolThreadForY77 = bool.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : isUsingThreadPoolThreadForY77;
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogHelper.Logger.LogError(ex);
                }
            }
        }
    }
}
