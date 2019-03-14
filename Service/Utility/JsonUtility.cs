using System.IO;
using Newtonsoft.Json;
using RestSharp.Serialization.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Service {
    public static class JsonUtility {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();
        private static readonly JsonDeserializer JsonDeserializer = new JsonDeserializer();

        /// <summary>
        /// Json deserializer for generic types
        /// </summary>
        public static T Deserialize<T>(string json) {
            return JsonSerializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        /// <summary>
        /// Json serializer for generic types
        /// </summary>
        public static string Serialize<T>(T @object) {
            return JsonDeserializer.Serialize(@object);
        }
    }
}