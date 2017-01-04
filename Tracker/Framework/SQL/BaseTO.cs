using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Xml;
using System.Data.SqlClient;



namespace Framework.SQL
{
     [Serializable]
    public class BaseTO
    {
        public const int _DEFAULTNULL = -214748364;
        public const int _INTNULL = -214748363;
        public static readonly DateTime _DATENULL = DateTime.Parse("0002-1-1 0:00:00");


        public void InitData()
        {
            //SqlActionBuilder<BaseTO>.InitNullValue(this);
        }

        public void Initialize(SqlDataReader reader)
        {
            Type type = this.GetType();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string field = reader.GetName(i);
                if (field.Equals("rownum"))
                    continue;
                PropertyInfo info = type.GetProperty(field);
#if DEBUG
                try
                {
#endif
                    if (info != null)
                    {
                        object value = reader.GetValue(i);
                        if (value != null && value != DBNull.Value)
                            info.SetValue(this, value, null);
                    }
#if DEBUG
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot Init TO: field " + field, e);
                }
#endif
            }
        }

        public void Initialize(Dictionary<string, object> reader)
        {
            Type type = this.GetType();
            foreach (string key in reader.Keys)
            {
                if (key.Equals("rownum"))
                    continue;
                PropertyInfo info = type.GetProperty(key);
                if (info != null)
                {
                    object value = reader[key];
                    if (value != null && value != DBNull.Value)
                        info.SetValue(this, value, null);
                }
            }
        }

        public void Initialize(XmlNode node)
        {
            Type type = this.GetType();
            foreach (XmlNode child in node.ChildNodes)
            {
                PropertyInfo info = type.GetProperty(child.Name);
                if (info != null)
                {
                    object value = getTypeValue(info.PropertyType, child.InnerText);
                    if (value != null)
                    {
                        try
                        {
                            if (info.PropertyType == typeof(System.Guid))//reason: when type of value in alias to is guid, using 'info.SetValue(this, value, null)' will cause "string can not convert to guid" error
                            {//no need to add tryparse, throw exception when type is guid but value is not
                                Guid guidvalue = new Guid(value.ToString());
                                info.SetValue(this, guidvalue, null);
                            }
                            else //others when type is not Guid
                                info.SetValue(this, value, null);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Error when setting: " + info.PropertyType.Name + ", " + child.Name + " = " + child.InnerText, e.InnerException);
                        }
                    }
                }
            }
        }

        
        private object getTypeValue(Type type, string value) //wilsontang can i move this to common dbUtilies?
        {
            object returnValue = null;

            switch (type.Name.ToLower())
            {
                case "int64":
                    returnValue = long.Parse(value);
                    break;
                case "byte":
                    returnValue = byte.Parse(value);
                    break;
                case "int32":
                case "int16":
                    returnValue = int.Parse(value);
                    break;
                case "decimal":
                    returnValue = Convert.ToDecimal(value);
                    break;
                case "double":
                    returnValue = Convert.ToDouble(value);
                    break;
                case "datetime":
                    returnValue = Convert.ToDateTime(value);
                    break;
                case "boolean":
                    returnValue = Convert.ToBoolean(Convert.ToInt32(value));
                    break;
                case "string":
                    returnValue = value;
                    break;
                default:
                    if (type.IsValueType)
                    {
                        returnValue = value;
                    }
                    break;
            }

            return returnValue;

        }

    }
}
