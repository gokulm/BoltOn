using System.Text.Json;

namespace BoltOn.Cache
{
    public interface IBoltOnCacheSerializer
    {
        byte[] ToByteArray(object obj);
        T FromByteArray<T>(byte[] byteArray) where T : class;
    }

    public class BoltOnCacheSerializer : IBoltOnCacheSerializer
    {
        public T FromByteArray<T>(byte[] byteArray) where T : class
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
