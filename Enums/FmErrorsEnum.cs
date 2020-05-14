using System;

namespace CSBEF.Module.UserCloudManager.Enums
{
    public enum FmErrorsEnum
    {
        AddDirectory_ModelValidationFail,
        AddDirectory_ParentDirectoryNotFound,
        AddDirectory_TitleExists,
        UpdateDirectory_ModelValidationFail,
        UpdateDirectory_DataNotFound,
        UpdateDirectory_TitleExists,
        ChangeStatusDirectory_ModelValidationFail,
        ChangeStatusDirectory_DataNotFound,
        MoveDirectory_ModelValidationFail,
        MoveDirectory_MovedDirectoryNotFound,
        MoveDirectory_TargetDirectoryNotFound,
        MoveDirectory_NestingProblem,
        CopyDirectory_TargetDirectoryNotFound,
        CopyDirectory_CopiedDirectoryNotFound,
        CopyDirectory_ModelValidationFail,
        ShareDirectory_ModelValidationFail,
        ShareDirectory_DataNotFound,
        AddFile_ModelValidationFail,
        AddFile_DirectoryNotFound,
        AddFile_TitleExists,
        UpdateFile_ModelValidationFail,
        UpdateFile_DataNotFound,
        UpdateFile_TitleExists,
        ChangeStatusFile_ModelValidationFail,
        ChangeStatusFile_DataNotFound,
        MoveFile_ModelValidationFail,
        MoveFile_FileNotFound,
        MoveFile_DirectoryNotFound,
        CopyFile_ModelValidationFail,
        CopyFile_FileNotFound,
        CopyFile_DirectoryNotFound,
        ShareFile_ModelValidationFail,
        ShareFile_DataNotFound,
        GetFmTreeDirectoryItem_DirectoryNotFound,
        GetFmTreeFileItem_FileNotFound,
        GetShareFileItem_FileNotFound,
        GetShareFileItem_DirectoryNotFound
    }
}
