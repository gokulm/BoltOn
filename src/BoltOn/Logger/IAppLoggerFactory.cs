namespace BoltOn.Logger
{
    public interface IAppLoggerFactory
    {
        IAppLogger<TType> Create<TType>();
    }
}
