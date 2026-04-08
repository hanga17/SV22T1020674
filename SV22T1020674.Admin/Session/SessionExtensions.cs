using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SV22T1020674.Session
{
    public static class SessionExtensions
    {
        // LƯU OBJECT
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // LẤY OBJECT
        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}