using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BoltOn.Tests.Caching.Fakes
{
	public class CacheTestHelper
	{
		public static byte[] ToByteArray(object obj)
		{
			if (obj == null)
			{
				return null;
			}

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}

		public static T FromByteArray<T>(byte[] byteArray) where T : class
		{
			if (byteArray == null)
				return default;

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream(byteArray);
			return binaryFormatter.Deserialize(memoryStream) as T;
		}
	}
}
