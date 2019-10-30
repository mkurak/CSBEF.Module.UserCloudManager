using CSBEF.Core.Abstracts;
using CSBEF.Core.Concretes;
using CSBEF.Module.UserCloudManager.Interfaces.Repository;
using CSBEF.Module.UserCloudManager.Models.Poco;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSBEF.Module.UserCloudManager.Repositories
{
    public class FmFileRepository : RepositoryBase<FmFile>, IFmFileRepository
    {
        public FmFileRepository(ModularDbContext context) : base(context)
        {

        }
    }
}
