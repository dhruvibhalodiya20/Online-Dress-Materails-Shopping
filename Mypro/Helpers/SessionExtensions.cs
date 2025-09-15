using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Mypro.Helpers
{
    public static class SessionExtensions
    {
        // Save object into Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Get object from Session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
