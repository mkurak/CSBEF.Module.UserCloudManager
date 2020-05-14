using CSBEF.Core.Abstracts;
using CSBEF.Core.Concretes;
using CSBEF.Module.UserCloudManager.Interfaces.Repository;
using CSBEF.Module.UserCloudManager.Models.Poco;
using System;

namespace CSBEF.Module.UserCloudManager.Repositories
{
    public class FmShareRepository : RepositoryBase<FmShare>, IFmShareRepository
    {
        public FmShareRepository(ModularDbContext context) : base(context)
        {

        }
    }
}
