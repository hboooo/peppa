using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Xml.Linq;

namespace Peppa
{
    public class DynamicJson
    {
        static JsonSerializerSettings mJsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        public static String SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, mJsonSettings);
        }

        public static T DeserializeObject<T>(string value)
        {
            var iso = new IsoDateTimeConverter();
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
            return JsonConvert.DeserializeObject<T>(value, iso);
        }

        public static T DeserializeXmlObject<T>(string xml)
        {
            XDocument xmlDoc = XDocument.Parse(xml);
            string val = JsonConvert.SerializeXNode(xmlDoc.Root);
            return DeserializeObject<T>(val);
        }
    }
}
