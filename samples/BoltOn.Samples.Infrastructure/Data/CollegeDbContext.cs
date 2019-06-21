using BoltOn.Data.CosmosDb;
using Microsoft.Extensions.Options;

namespace BoltOn.Samples.Infrastructure.Data
{
    public class CollegeDbContext : BaseCosmosDbContext
    {
        //private readonly CosmosSettings _settings;
        private const string DatabaseName = "College";

        public CollegeDbContext(IOptions<CosmosDbSettings> settings) : base(settings.Value, DatabaseName)
        {
            // _settings = settings.Value;
        }

        protected override void SetCosmosDbSetting()
        {
            // If need to override setting
            //CosmosSetting = _settings.CosmosDbs[DatabaseName]; 
        }
    }
}
