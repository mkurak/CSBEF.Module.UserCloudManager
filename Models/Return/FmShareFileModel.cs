using System;

namespace CSBEF.Module.UserCloudManager.Models.Return
{
    public class FmShareFileModel
    {
        public int FileId { get; set; }
        public int OwnerUserId { get; set; }
        public int SharedUserId { get; set; }
        public string FileTitle { get; set; }
        public string FileDescription { get; set; }
        public long FileSize { get; set; }
        public string FileFullPath { get; set; }
        public string FilePath { get; set; }
        public string DirectoryTitle { get; set; }
    }
}
