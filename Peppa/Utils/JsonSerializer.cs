using RestSharp.Serializers;

namespace Peppa
{
    public class JsonSerializer : ISerializer
    {
        private string contentType = "application/json";

        public string Serialize(object obj)
        {
            return DynamicJson.SerializeObject(obj);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }

        public string ContentType
        {
            get { return contentType; }
            set { this.contentType = value; }
        }
    }
}