namespace BoltOn.Data.CosmosDb
{
	public abstract class BaseCosmosDbContext<TCosmosDbContext> where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
    {
		public CosmosDbContextOptions<TCosmosDbContext> Options { get; private set; }

		protected BaseCosmosDbContext(CosmosDbContextOptions<TCosmosDbContext> options)
        {
			Options = options;
        }
    }
}
