using CSBEF.Module.UserCloudManager.Models.DTO;
using System;
using System.Collections.Generic;

namespace CSBEF.Module.UserCloudManager.Models.Return
{
    public class FmTreeItemSharedModel
    {
        public List<FmShareDTO> Groups { get; } = new List<FmShareDTO>();
        public List<FmShareDTO> Users { get; } = new List<FmShareDTO>();
    }
}
