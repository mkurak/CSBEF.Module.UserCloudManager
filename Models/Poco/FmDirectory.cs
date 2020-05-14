using CSBEF.Core.Models;
using System;

namespace CSBEF.Module.UserCloudManager.Models.Poco
{
    public partial class FmDirectory : EntityModelBase
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
