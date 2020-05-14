using CSBEF.Core.Models;
using System;

namespace CSBEF.Module.UserCloudManager.Models.DTO
{
    public class FmDirectoryDTO : DTOModelBase
    {
        public int UserId { get; set; }
        public int? ParentDirectoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public Guid FsKey { get; set; }
        public long Size { get; set; }
        public int FileCount { get; set; }
    }
}
