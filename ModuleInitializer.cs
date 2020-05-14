using CSBEF.Core.Interfaces;
using CSBEF.Module.UserCloudManager.Interfaces.Repository;
using CSBEF.Module.UserCloudManager.Interfaces.Service;
using CSBEF.Module.UserCloudManager.Repositories;
using CSBEF.Module.UserCloudManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CSBEF.Module.UserCloudManager
{
    public class ModuleInitializer : IModuleInitializer
    {
        public void Init(IServiceCollection services)
        {
            #region Repositories

            services.AddScoped<IFmDirectoryRepository, FmDirectoryRepository>();
            services.AddScoped<IFmFileRepository, FmFileRepository>();
            services.AddScoped<IFmShareRepository, FmShareRepository>();

            #endregion Repositories

            #region Services

            services.AddScoped<IFmService, FmService>();

            #endregion Services
        }
    }
}
