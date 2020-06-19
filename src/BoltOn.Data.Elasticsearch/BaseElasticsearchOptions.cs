using Nest;

namespace BoltOn.Data.Elasticsearch
{
	public abstract class BaseElasticsearchOptions
	{
		public ConnectionSettings ConnectionSettings { get; set; }
	}
}
