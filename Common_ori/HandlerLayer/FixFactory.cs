using Common.LinkLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;


namespace Common.HandlerLayer
{
    public class FixFactory
    {
        public static List<List<MessageField>> ConvertTableToMessage(Type DataType, DataTable Data)
        {
            List<List<MessageField>> MqMessageRows = new List<List<MessageField>>();
            foreach (DataRow DR in Data.Rows)
            {
                List<MessageField> MqMessageRow = new List<MessageField>();
                string ConstVal = "";
                string ColumnVal = "";
                foreach (DataColumn DC in Data.Columns)
                {
                    ConstVal = GetConstantValueInClass(DC.ColumnName, DataType);
                    ColumnVal = DBNull.Value.Equals(DR[DC.ColumnName]) ? "" : DR[DC.ColumnName].ToString();

                    if (ConstVal != "")
                    {
                        MessageField MessageField = new MessageField();
                        MessageField.Name = ConstVal;
                        MessageField.Value = ColumnVal;
                        MqMessageRow.Add(MessageField);
                    }
                }
                if (MqMessageRow.Count > 0)
                {
                    MqMessageRows.Add(MqMessageRow);
                }
            }
            return MqMessageRows;
        }

        private static string GetConstantValueInClass(string ConstName, System.Type type)
        {
            string ConstVal = "";

            // Gets all public and static fields
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo fi in fieldInfos)
            {
                if (fi.IsLiteral && !fi.IsInitOnly && fi.Name.ToLower() == ConstName.ToLower())
                {
                    ConstVal = fi.GetValue(type).ToString();
                    break;
                }
            }
            //Gets property from entity class
            PropertyInfo[] popertyInfos = type.GetProperties();
            foreach (PropertyInfo pi in popertyInfos)
            {
                if (pi.Name.ToLower() == ConstName.ToLower())
                {
                    ConstVal = pi.Name;
                    break;
                }
            }
            return ConstVal;
        }
    }
}
