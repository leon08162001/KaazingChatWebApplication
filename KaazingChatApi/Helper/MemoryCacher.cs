using System;
using System.Runtime.Caching;

namespace KaazingChatWebApplication.Helper
{
    public static class MemoryCacher
    {
        public static object GetValue(string key)
        {
            key = key.ToUpper();
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Get(key);
        }

        public static bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            key = key.ToUpper();
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Add(key, value, absExpiration);
        }
        public static void Update(string key, object value, DateTimeOffset absExpiration)
        {
            key = key.ToUpper();
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Set(key, value, absExpiration);
            }
        }
        public static void Delete(string key)
        {
            key = key.ToUpper();
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Remove(key);
            }
        }
        public static bool Exist(string key)
        {
            key = key.ToUpper();
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Contains(key);
        }
    }
}