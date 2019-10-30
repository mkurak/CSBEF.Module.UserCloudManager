using CSBEF.Core.Concretes;
using CSBEF.Core.Interfaces;
using CSBEF.Module.UserCloudManager.Models.DTO;
using CSBEF.Module.UserCloudManager.Models.Request;
using CSBEF.Module.UserCloudManager.Models.Return;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSBEF.Module.UserCloudManager.Interfaces.Service
{
    public interface IFmService : IServiceBase
    {
        IReturnModel<FmDirectoryDTO> AddDirectory(ServiceParamsWithIdentifier<FmDirectoryAddModel> args);
        IReturnModel<FmDirectoryDTO> UpdateDirectory(ServiceParamsWithIdentifier<FmDirectoryUpdateModel> args);
        IReturnModel<bool> ChangeStatusDirectory(ServiceParamsWithIdentifier<ChangeStatusModel> args);
        IReturnModel<bool> MoveDirectory(ServiceParamsWithIdentifier<DirectoryMoveModel> args);
        IReturnModel<bool> CopyDirectory(ServiceParamsWithIdentifier<DirectoryCopModel> args);
        IReturnModel<bool> ShareDirectory(ServiceParamsWithIdentifier<FmDirectoryShareModel> args);
        IReturnModel<FmFileDTO> AddFile(ServiceParamsWithIdentifier<FmFileAddModel> args);
        IReturnModel<FmFileDTO> UpdateFile(ServiceParamsWithIdentifier<FmFileUpdateModel> args);
        IReturnModel<bool> ChangeStatusFile(ServiceParamsWithIdentifier<ChangeStatusModel> args);
        IReturnModel<FmFileDTO> MoveFile(ServiceParamsWithIdentifier<FmFileMoveModel> args);
        IReturnModel<FmFileDTO> CopyFile(ServiceParamsWithIdentifier<FmFileCopyModel> args);
        IReturnModel<bool> ShareFile(ServiceParamsWithIdentifier<FmFileShareModel> args);
        IReturnModel<IList<FmTreeItemModel>> FmTreeItems(ServiceParamsWithIdentifier<int> args);
        IReturnModel<IList<FmShareFileModel>> FmSharedFiles(ServiceParamsWithIdentifier<int> args);
    }
}
