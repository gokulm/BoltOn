using BoltOn.Data.Cosmos;
using Microsoft.Extensions.Options;

namespace BoltOn.Samples.Infrastructure.Data
{
    public class CollegeDbContext : BaseCosmosContext
    {
        //private readonly CosmosSettings _settings;
        private const string DatabaseName = "College";

        public CollegeDbContext(IOptions<CosmosSettings> settings) : base(settings.Value, DatabaseName)
        {
            // _settings = settings.Value;
        }

        protected override void SetCosmosSetting()
        {
            // If need to override setting
            //CosmosSetting = _settings.CosmosDbs[DatabaseName]; 
        }
    }
}
