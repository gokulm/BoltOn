using System.Text.Json;

namespace BoltOn.Cache
{
    public interface IAppCacheSerializer
    {
        byte[] ToByteArray(object obj);
        T FromByteArray<T>(byte[] byteArray);
    }

    public class AppCacheSerializer : IAppCacheSerializer
    {
        public T FromByteArray<T>(byte[] byteArray)
        {
            if (byteArray == default || byteArray.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(byteArray);
        }

        public byte[] ToByteArray(object obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
    }
}
