using CSBEF.Core.Models;
using System;

namespace CSBEF.Module.UserCloudManager.Models.Poco
{
    public partial class FmFile : EntityModelBase
    {
        public int UserId { get; set; }
        public int DirectoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FsKey { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
    }
}
