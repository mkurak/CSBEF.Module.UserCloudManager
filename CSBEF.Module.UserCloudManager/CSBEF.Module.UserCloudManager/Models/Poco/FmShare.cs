using CSBEF.Core.Models;

namespace CSBEF.Module.UserCloudManager.Models.Poco
{
    public partial class FmShare : EntityModelBase
    {
        public int? FileId { get; set; }
        public int? DirectoryId { get; set; }
        public int? SharedGroupId { get; set; }
        public int? SharedUserId { get; set; }
    }
}
