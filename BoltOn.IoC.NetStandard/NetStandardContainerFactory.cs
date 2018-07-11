namespace BoltOn.IoC.NetStandardBolt
{
	public class NetStandardContainerFactory : IBoltOnContainerFactory
    {
        public IBoltOnContainer Create()
        {
			return new NetStandardContainerAdapter();
        }
	}
}
