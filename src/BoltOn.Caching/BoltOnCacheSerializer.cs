using System.Text.Json;

namespace BoltOn.Caching
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
            if (byteArray == null || byteArray.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(byteArray);
        }

        public byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;

            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
    }
}
