using CSBEF.Module.UserCloudManager.Models.DTO;
using System;
using System.Collections.Generic;

namespace CSBEF.Module.UserCloudManager.Models.Return
{
    public class FmTreeItemModel
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public string ItemType { get; set; } // "directory", "file"
        public FmDirectoryDTO DirectoryData { get; set; }
        public FmFileDTO FileData { get; set; }
        public List<FmTreeItemModel> Children { get; } = new List<FmTreeItemModel>();
        public string FullDirectoryPath { get; set; }
        public FmTreeItemSharedModel Shared { get; set; }
    }
}
