using AutoMapper;
using CSBEF.Module.UserCloudManager.Models.DTO;
using CSBEF.Module.UserCloudManager.Models.Poco;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSBEF.Module.UserCloudManager
{
    public class AutoMapperInitializer : Profile
    {
        public AutoMapperInitializer()
        {
            #region POCO => POCO

            CreateMap<FmDirectory, FmDirectory>();
            CreateMap<FmFile, FmFile>();
            CreateMap<FmShare, FmShare>();

            #endregion POCO => POCO

            #region DTO => DTO

            CreateMap<FmDirectoryDTO, FmDirectoryDTO>();
            CreateMap<FmFileDTO, FmFileDTO>();
            CreateMap<FmShareDTO, FmShareDTO>();

            #endregion DTO => DTO

            #region POCO => DTO & DTO => POCO

            CreateMap<FmDirectory, FmDirectoryDTO>().ReverseMap();
            CreateMap<FmFile, FmFileDTO>().ReverseMap();
            CreateMap<FmShare, FmShareDTO>().ReverseMap();

            #endregion POCO => DTO & DTO => POCO
        }
    }
}
