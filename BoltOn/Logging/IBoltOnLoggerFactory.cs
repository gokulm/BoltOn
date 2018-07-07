namespace BoltOn.Logging
{
    public interface IBoltOnLoggerFactory
    {
        IBoltOnLogger<TType> Create<TType>();
    }
}
