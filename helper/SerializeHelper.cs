using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RCApp_Win.Logic.Utility
{
    public class SerializeHelper
    {
        public static string SerializeObjectToJson<T>(T t)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //return serializer.Serialize(t);
            if (t == null)
            {
                return null;
            }
            try
            {
                return JsonConvert.SerializeObject(t);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string SerializeObjectToXml<T>(T t)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            using (MemoryStream mem = new MemoryStream())
            using (XmlTextWriter writer = new XmlTextWriter(mem, Encoding.UTF8))
            {
                //ns.Add("", "");
                serializer.Serialize(writer, t, ns);
                return Encoding.UTF8.GetString(mem.ToArray());
            }
        }

        public static T DeserializeJsonToObject<T>(string json)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //return serializer.Deserialize<T>(json);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        public static T DeserializeXmlToObject<T>(string xml)
        {
            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.LoadXml(xml);
                XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(reader);
                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }

        public static DataTable JSONStringToDataTable<T>(string json)
        {
            DataTable dt = new DataTable();
            if (json.IndexOf("[") > -1)//如果大于则strJson存放了多个model对象
            {
                json = json.Remove(json.Length - 1, 1).Remove(0, 1).Replace("},{", "};{");
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string[] items = json.Split(';');

            foreach (PropertyInfo property in typeof(T).GetProperties())//通过反射获得T类型的所有属性
            {
                DataColumn col = new DataColumn(property.Name, property.PropertyType);
                dt.Columns.Add(col);
            }
            //循环 一个一个的反序列化
            for (int i = 0; i < items.Length; i++)
            {
                DataRow dr = dt.NewRow();
                //反序列化为一个T类型对象
                T temp = serializer.Deserialize<T>(items[i]);
                foreach (PropertyInfo property in typeof(T).GetProperties())
                {
                    dr[property.Name] = property.GetValue(temp, null);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
