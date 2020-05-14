using AutoMapper;
using CSBEF.Core.Abstracts;
using CSBEF.Core.Concretes;
using CSBEF.Core.Enums;
using CSBEF.Core.Helpers;
using CSBEF.Core.Interfaces;
using CSBEF.Core.Models;
using CSBEF.Core.Models.HubModels;
using CSBEF.Module.UserCloudManager.Enums;
using CSBEF.Module.UserCloudManager.Interfaces.Repository;
using CSBEF.Module.UserCloudManager.Interfaces.Service;
using CSBEF.Module.UserCloudManager.Models.DTO;
using CSBEF.Module.UserCloudManager.Models.Poco;
using CSBEF.Module.UserCloudManager.Models.Request;
using CSBEF.Module.UserCloudManager.Models.Return;
using CSBEF.Module.UserManagement.Interfaces.Repository;
using CSBEF.Module.UserManagement.Poco;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace CSBEF.Module.UserCloudManager.Services
{
    public class FmService : ServiceBase, IFmService
    {
        #region Private Variables

        private readonly string _moduleName = "Project";
        private readonly string _serviceName = "FmService";
        private readonly string _storageRootPath = string.Empty;

        #endregion Private Variables

        #region Dependencies

        private readonly IConfiguration _configuration;
        private readonly ILogger<ILog> _logger;
        private readonly IMapper _mapper;
        private readonly IEventService _eventService;
        private readonly IHubSyncDataService _hubSyncDataService;
        private readonly IFmDirectoryRepository _fmDirectoryRepository;
        private readonly IFmFileRepository _fmFileRepository;
        private readonly IFmShareRepository _fmShareRepository;
        private readonly IUserInGroupRepository _userInGroupRepository;

        #endregion Dependencies

        #region ctor

        public FmService(
            IConfiguration configuration,
            ILogger<ILog> logger,
            IMapper mapper,
            IEventService eventService,
            IHubSyncDataService hubSyncDataService,
            IFmDirectoryRepository fmDirectoryRepository,
            IFmFileRepository fmFileRepository,
            IFmShareRepository fmShareRepository,
            IUserInGroupRepository userInGroupRepository
        )
        {
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _eventService = eventService;
            _hubSyncDataService = hubSyncDataService;
            _fmDirectoryRepository = fmDirectoryRepository;
            _fmFileRepository = fmFileRepository;
            _fmShareRepository = fmShareRepository;
            _userInGroupRepository = userInGroupRepository;

            string storagePath = string.Empty;
            if (_configuration != null)
            {
                if (_configuration["AppSettings:FileUploader:Modules:UserCloudManager:Fm:Storage"] != null)
                {
                    storagePath = _configuration["AppSettings:FileUploader:Modules:UserCloudManager:Fm:Storage"];
                }
            }

            _storageRootPath = Path.Combine(GlobalConfiguration.SWwwRootPath, storagePath);
        }

        #endregion ctor

        #region Actions

        public IReturnModel<FmDirectoryDTO> AddDirectory(ServiceParamsWithIdentifier<FmDirectoryAddModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<FmDirectoryDTO> rtn = new ReturnModel<FmDirectoryDTO>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<FmDirectoryDTO>, ServiceParamsWithIdentifier<FmDirectoryAddModel>> afterEventParameterModel = null;
                IReturnModel<FmDirectoryDTO> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getParentDirectory = null;
                FmDirectory checkTitle = null;
                FmDirectory newItem = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.AddDirectory.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmDirectoryAddModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.AddDirectory_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt && args.Param.ParentDirectoryId.ToInt(0) > 0)
                {
                    getParentDirectory = _fmDirectoryRepository.Find(i => i.Id == args.Param.ParentDirectoryId);
                    if (getParentDirectory == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.AddDirectory_ParentDirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    checkTitle = _fmDirectoryRepository.Find(i => i.ParentDirectoryId == args.Param.ParentDirectoryId && i.Title == args.Param.Title && i.Status == true && i.UserId == args.Param.UserId);
                    if (checkTitle != null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.AddDirectory_TitleExists);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    newItem = new FmDirectory
                    {
                        UserId = args.Param.UserId,
                        ParentDirectoryId = args.Param.ParentDirectoryId,
                        Title = args.Param.Title,
                        Description = args.Param.Description,
                        Path = "/",
                        FsKey = Guid.NewGuid(),
                        Size = 0,
                        FileCount = 0,
                        Status = true,
                        AddingDate = DateTime.Now,
                        UpdatingDate = DateTime.Now,
                        AddingUserId = args.UserId,
                        UpdatingUserId = args.UserId
                    };

                    newItem = _fmDirectoryRepository.Add(newItem);
                    _fmDirectoryRepository.Save();

                    newItem.Path = getParentDirectory != null ? getParentDirectory.Path + "/" + newItem.Id : newItem.Id.ToString();
                    newItem = _fmDirectoryRepository.Update(newItem);
                    _fmDirectoryRepository.Save();

                    Directory.CreateDirectory(Path.Combine(_storageRootPath, newItem.Path));

                    rtn.Result = _mapper.Map<FmDirectoryDTO>(newItem);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "add",
                        Id = rtn.Result.Id,
                        UserId = args.UserId,
                        Name = rtn.Result.Title,
                        Data = (GetFmTreeDirectoryItem(rtn.Result.Id)).Result
                    });
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<FmDirectoryDTO>, ServiceParamsWithIdentifier<FmDirectoryAddModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "AddDirectory"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.AddDirectory.After")
                        .EventHandler<FmDirectoryDTO, IAfterEventParameterModel<IReturnModel<FmDirectoryDTO>, ServiceParamsWithIdentifier<FmDirectoryAddModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;
                getParentDirectory = null;
                checkTitle = null;
                newItem = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<FmDirectoryDTO> UpdateDirectory(ServiceParamsWithIdentifier<FmDirectoryUpdateModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<FmDirectoryDTO> rtn = new ReturnModel<FmDirectoryDTO>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<FmDirectoryDTO>, ServiceParamsWithIdentifier<FmDirectoryUpdateModel>> afterEventParameterModel = null;
                IReturnModel<FmDirectoryDTO> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getItem = null;
                FmDirectory checkTitle = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.UpdateDirectory.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmDirectoryUpdateModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.UpdateDirectory_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getItem = _fmDirectoryRepository.Find(i => i.Id == args.Param.Id);
                    if (getItem == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.UpdateDirectory_DataNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    checkTitle = _fmDirectoryRepository.Find(i => i.Id != args.Param.Id && i.ParentDirectoryId == getItem.ParentDirectoryId && i.Title == args.Param.Title && i.Status == true);
                    if (checkTitle != null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.UpdateDirectory_TitleExists);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getItem.Title = args.Param.Title;
                    getItem.Description = args.Param.Description;
                    getItem.UpdatingDate = DateTime.Now;
                    getItem.UpdatingUserId = args.UserId;
                    getItem = _fmDirectoryRepository.Update(getItem);
                    _fmDirectoryRepository.Save();

                    rtn.Result = _mapper.Map<FmDirectoryDTO>(getItem);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = rtn.Result.Id,
                        UserId = args.UserId,
                        Name = rtn.Result.Title,
                        Data = (GetFmTreeDirectoryItem(rtn.Result.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<FmDirectoryDTO>, ServiceParamsWithIdentifier<FmDirectoryUpdateModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "UpdateDirectory"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.UpdateDirectory.After")
                        .EventHandler<FmDirectoryDTO, IAfterEventParameterModel<IReturnModel<FmDirectoryDTO>, ServiceParamsWithIdentifier<FmDirectoryUpdateModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;
                getItem = null;
                checkTitle = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<bool> ChangeStatusDirectory(ServiceParamsWithIdentifier<ChangeStatusModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<ChangeStatusModel>> afterEventParameterModel = null;
                IReturnModel<bool> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getData = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ChangeStatusDirectory.Before").EventHandler<bool, ServiceParamsWithIdentifier<ChangeStatusModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ChangeStatusDirectory_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getData = _fmDirectoryRepository.Find(i => i.Id == args.Param.Id);
                    if (getData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ChangeStatusDirectory_DataNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getData.Status = args.Param.Status;
                    getData.UpdatingDate = DateTime.Now;
                    getData.UpdatingUserId = args.UserId;
                    getData = _fmDirectoryRepository.Update(getData);
                    _fmDirectoryRepository.Save();

                    ChangeStatusFmDirectoryDeep(getData.Id, args.UserId, args.Param.Status);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "remove",
                        Id = args.Param.Id,
                        UserId = args.UserId,
                        Name = getData.Title,
                        Data = (GetFmTreeDirectoryItem(args.Param.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                rtn.Result = cnt;

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<ChangeStatusModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "ChangeStatus"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ChangeStatusDirectory.After")
                        .EventHandler<bool, IAfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<ChangeStatusModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;
                getData = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<bool> MoveDirectory(ServiceParamsWithIdentifier<DirectoryMoveModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<DirectoryMoveModel>> afterEventParameterModel = null;
                IReturnModel<bool> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getMovedDirectory = null;
                FmDirectory getTargetDirectory = null;
                int checkMovedTitle = 0;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.MoveDirectory.Before").EventHandler<bool, ServiceParamsWithIdentifier<DirectoryMoveModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.MoveDirectory_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getMovedDirectory = _fmDirectoryRepository.Find(i => i.Id == args.Param.MovedDirectoryId);
                    if (getMovedDirectory == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.MoveDirectory_MovedDirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getTargetDirectory = _fmDirectoryRepository.Find(i => i.Id == args.Param.TargetDirectoryId);
                    if (getTargetDirectory == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.MoveDirectory_TargetDirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt && getTargetDirectory.Path.StartsWith(getMovedDirectory.Path))
                {
                    rtn = rtn.SendError(FmErrorsEnum.MoveDirectory_NestingProblem);
                    cnt = false;
                }

                if (cnt)
                {
                    checkMovedTitle = _fmDirectoryRepository.Count(i => i.ParentDirectoryId == getTargetDirectory.Id && i.Title == getMovedDirectory.Title);
                    if (checkMovedTitle > 0)
                    {
                        getMovedDirectory.Title += " (2)";
                    }

                    Directory.Move(Path.Combine(_storageRootPath, getMovedDirectory.Path), Path.Combine(_storageRootPath, (getTargetDirectory.Path + "/" + getMovedDirectory.Id)));

                    getMovedDirectory.ParentDirectoryId = args.Param.TargetDirectoryId;
                    getMovedDirectory.Path = getTargetDirectory.Path + "/" + getMovedDirectory.Id;
                    getMovedDirectory.UpdatingDate = DateTime.Now;
                    getMovedDirectory.UpdatingUserId = args.UserId;
                    _fmDirectoryRepository.Update(getMovedDirectory);
                    _fmDirectoryRepository.Save();

                    ChangePathDirectoryDeep(getMovedDirectory.Path, getMovedDirectory.Id, args.UserId);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getMovedDirectory.Id,
                        UserId = args.UserId,
                        Name = getMovedDirectory.Title,
                        Data = (GetFmTreeDirectoryItem(getMovedDirectory.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getTargetDirectory.Id,
                        UserId = args.UserId,
                        Name = getTargetDirectory.Title,
                        Data = (GetFmTreeDirectoryItem(getTargetDirectory.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                rtn.Result = cnt;

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<DirectoryMoveModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "MoveDirectory"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.MoveDirectory.After")
                        .EventHandler<bool, IAfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<DirectoryMoveModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<bool> CopyDirectory(ServiceParamsWithIdentifier<DirectoryCopModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<DirectoryCopModel>> afterEventParameterModel = null;
                IReturnModel<bool> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getCopiedDirectory = null;
                FmDirectory getTargetDirectory = null;
                string sourceDirName = null;
                string destinationDirName = null;
                int checkNewTitle = 0;
                FmDirectory newCopiedDirectoryModel = null;
                ICollection<FmDirectory> dataListCache = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.CopyDirectory.Before").EventHandler<bool, ServiceParamsWithIdentifier<DirectoryCopModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.CopyDirectory_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getCopiedDirectory = _fmDirectoryRepository.Find(i => i.Id == args.Param.CopiedDirectoryId);
                    if (getCopiedDirectory == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.CopyDirectory_CopiedDirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getTargetDirectory = _fmDirectoryRepository.Find(i => i.Id == args.Param.TargetDirectoryId);
                    if (getTargetDirectory == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.CopyDirectory_TargetDirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    dataListCache = _fmDirectoryRepository.FindAll(i => i.Path.StartsWith(getCopiedDirectory.Path));

                    sourceDirName = Path.Combine(_storageRootPath, getCopiedDirectory.Path);
                    destinationDirName = Path.Combine(_storageRootPath, (getTargetDirectory.Path + "/" + getCopiedDirectory.Id.ToString()));

                    checkNewTitle = _fmDirectoryRepository.Count(i => i.ParentDirectoryId == getTargetDirectory.Id && i.Title == getCopiedDirectory.Title);

                    newCopiedDirectoryModel = new FmDirectory
                    {
                        UserId = getCopiedDirectory.UserId,
                        ParentDirectoryId = getTargetDirectory.Id,
                        Title = checkNewTitle == 0 ? getCopiedDirectory.Title : getCopiedDirectory.Title + " (2)",
                        Description = getCopiedDirectory.Description,
                        Path = "/",
                        FsKey = Guid.NewGuid(),
                        Size = getCopiedDirectory.Size,
                        FileCount = getCopiedDirectory.FileCount,
                        Status = true,
                        AddingDate = DateTime.Now,
                        UpdatingDate = DateTime.Now,
                        AddingUserId = args.UserId,
                        UpdatingUserId = args.UserId
                    };

                    newCopiedDirectoryModel = _fmDirectoryRepository.Add(newCopiedDirectoryModel);
                    _fmDirectoryRepository.Save();

                    newCopiedDirectoryModel.Path = getTargetDirectory.Path + "/" + newCopiedDirectoryModel.Id;
                    newCopiedDirectoryModel = _fmDirectoryRepository.Update(newCopiedDirectoryModel);
                    _fmDirectoryRepository.Save();

                    Directory.CreateDirectory(Path.Combine(_storageRootPath, newCopiedDirectoryModel.Path));

                    DirectoryCopyData(dataListCache, newCopiedDirectoryModel.Id, getCopiedDirectory.Id, newCopiedDirectoryModel.Path, args.UserId);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getTargetDirectory.Id,
                        UserId = args.UserId,
                        Name = getTargetDirectory.Title,
                        Data = (GetFmTreeDirectoryItem(getTargetDirectory.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                rtn.Result = cnt;

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<DirectoryCopModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "CopyDirectory"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.CopyDirectory.After")
                        .EventHandler<bool, IAfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<DirectoryCopModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<bool> ShareDirectory(ServiceParamsWithIdentifier<FmDirectoryShareModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<FmDirectoryShareModel>> afterEventParameterModel = null;
                IReturnModel<bool> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getData = null;
                ICollection<FmShare> getSharedData = null;
                List<string> splitUsers = new List<string>();
                List<string> splitGroups = new List<string>();
                FmShare newShareData = null;
                List<int> shareUpUsers = new List<int>();
                List<int> shareUpGroups = new List<int>();
                List<int> shareDownUsers = new List<int>();
                List<int> shareDownGroups = new List<int>();
                List<int> upUsers = null;
                List<int> downUsers = null;
                ICollection<UserInGroup> getGroupsInUsers = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ShareDirectory.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmDirectoryShareModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ShareDirectory_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getData = _fmDirectoryRepository.Find(i => i.Id == args.Param.DirectoryId);
                    if (getData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ShareDirectory_DataNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getSharedData = _fmShareRepository.FindAll(i => i.DirectoryId == args.Param.DirectoryId);
                    if (!string.IsNullOrWhiteSpace(args.Param.SharedUserIds))
                    {
                        splitUsers = args.Param.SharedUserIds.Trim().Split(",").ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(args.Param.SharedGroupIds))
                    {
                        splitGroups = args.Param.SharedGroupIds.Trim().Split(",").ToList();
                    }

                    if (getSharedData.Any())
                    {
                        foreach (FmShare data in getSharedData)
                        {
                            if (data.SharedUserId.ToInt(0) > 0)
                            {
                                if (!splitUsers.Any(i => i.ToInt(0) == data.SharedUserId.ToInt(0)))
                                {
                                    _fmShareRepository.Delete(data);
                                    shareDownUsers.Add(data.SharedUserId.ToInt(0));
                                }
                            }

                            if (data.SharedGroupId.ToInt(0) > 0)
                            {
                                if (!splitGroups.Any(i => i.ToInt(0) == data.SharedGroupId.ToInt(0)))
                                {
                                    _fmShareRepository.Delete(data);
                                    shareDownGroups.Add(data.SharedGroupId.ToInt(0));
                                }
                            }
                        }

                        _fmShareRepository.Save();
                    }

                    if (splitUsers.Any())
                    {
                        foreach (string item in splitUsers)
                        {
                            if (_fmShareRepository.Count(i => i.DirectoryId == args.Param.DirectoryId && i.SharedUserId == item.ToInt(0)) == 0)
                            {
                                newShareData = new FmShare
                                {
                                    DirectoryId = getData.Id,
                                    SharedUserId = item.ToInt(0),
                                    Status = true,
                                    AddingDate = DateTime.Now,
                                    UpdatingDate = DateTime.Now,
                                    AddingUserId = args.UserId,
                                    UpdatingUserId = args.UserId
                                };
                                _fmShareRepository.Add(newShareData);
                                shareUpUsers.Add(item.ToInt(0));
                            }
                        }

                        _fmShareRepository.Save();
                    }

                    if (splitGroups.Any())
                    {
                        foreach (string item in splitGroups)
                        {
                            if (_fmShareRepository.Count(i => i.DirectoryId == args.Param.DirectoryId && i.SharedGroupId == item.ToInt(0)) == 0)
                            {
                                newShareData = new FmShare
                                {
                                    DirectoryId = getData.Id,
                                    SharedGroupId = item.ToInt(0),
                                    Status = true,
                                    AddingDate = DateTime.Now,
                                    UpdatingDate = DateTime.Now,
                                    AddingUserId = args.UserId,
                                    UpdatingUserId = args.UserId
                                };
                                _fmShareRepository.Add(newShareData);
                                shareUpGroups.Add(item.ToInt(0));
                            }
                        }

                        _fmShareRepository.Save();
                    }
                }

                if (cnt)
                {
                    upUsers = new List<int>();
                    downUsers = new List<int>();

                    if (shareUpUsers.Any())
                    {
                        upUsers.AddRange(shareUpUsers);
                    }

                    if (shareDownUsers.Any())
                    {
                        downUsers.AddRange(shareDownUsers);
                    }

                    if (shareUpGroups.Any())
                    {
                        foreach (int data in shareUpGroups)
                        {
                            getGroupsInUsers = _userInGroupRepository.FindAll(i => i.GroupId == data);
                            if (getGroupsInUsers.Any())
                            {
                                upUsers.AddRange(getGroupsInUsers.Select(i => i.UserId));
                            }
                        }
                    }

                    if (shareDownGroups.Any())
                    {
                        foreach (int data in shareDownGroups)
                        {
                            getGroupsInUsers = _userInGroupRepository.FindAll(i => i.GroupId == data);
                            if (getGroupsInUsers.Any())
                            {
                                downUsers.AddRange(getGroupsInUsers.Select(i => i.UserId));
                            }
                        }
                    }

                    if (upUsers.Any())
                    {
                        upUsers.Remove(getData.UserId);

                        foreach (int user in upUsers.Distinct())
                        {
                            NotifyShareUpForUser(getData.Id, getData.Title, user, args.UserId);
                        }
                    }

                    if (downUsers.Any())
                    {
                        downUsers.Remove(getData.UserId);

                        foreach (int user in downUsers.Distinct())
                        {
                            NotifyShareDownForUser(getData.Id, getData.Title, user, args.UserId);
                        }
                    }
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getData.Id,
                        UserId = args.UserId,
                        Name = getData.Title,
                        Data = (GetFmTreeDirectoryItem(getData.Id)).Result
                    });
                }

                rtn.Result = cnt;

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<FmDirectoryShareModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "ShareDirectory"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ShareDirectory.After")
                        .EventHandler<bool, IAfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<FmDirectoryShareModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;
                getData = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<FmFileDTO> AddFile(ServiceParamsWithIdentifier<FmFileAddModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileAddModel>> afterEventParameterModel = null;
                IReturnModel<FmFileDTO> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmDirectory getDirectory = null;
                FmFile newFileModel = null;
                FileUploader fileUploader = null;
                IReturnModel<string> savePicture = null;
                string fileCurrentPath = null;
                string fileTargetPath = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.AddFile.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmFileAddModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.AddFile_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getDirectory = _fmDirectoryRepository.Find(i => i.Id == args.Param.DirectoryId);
                    if (getDirectory == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.AddFile_DirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    if ((_fmFileRepository.Count(i => i.DirectoryId == args.Param.DirectoryId && i.Title == args.Param.Title)) > 0)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.AddFile_TitleExists);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    newFileModel = new FmFile
                    {
                        UserId = args.Param.UserId,
                        DirectoryId = args.Param.DirectoryId,
                        Title = args.Param.Title,
                        Description = args.Param.Description,
                        FsKey = Guid.NewGuid(),
                        Path = "/",
                        Size = args.Param.File.Length,
                        Status = true,
                        AddingDate = DateTime.Now,
                        UpdatingDate = DateTime.Now,
                        AddingUserId = args.UserId,
                        UpdatingUserId = args.UserId
                    };
                    newFileModel = _fmFileRepository.Add(newFileModel);
                    _fmFileRepository.Save();

                    newFileModel.Path = getDirectory.Path + "/" + newFileModel.Id + Path.GetExtension(args.Param.File.FileName);
                    newFileModel = _fmFileRepository.Update(newFileModel);
                    _fmFileRepository.Save();

                    fileUploader = new FileUploader(_configuration, _logger, null, 0, 0, Path.Combine(_storageRootPath, getDirectory.Path));
                    savePicture = fileUploader.Upload(args.Param.File);
                    if (savePicture.Error.Status)
                    {
                        rtn.Error = savePicture.Error;
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    fileCurrentPath = Path.Combine(Path.Combine(_storageRootPath, getDirectory.Path), savePicture.Result);
                    fileTargetPath = Path.Combine(Path.Combine(_storageRootPath, getDirectory.Path), newFileModel.Id.ToString());
                    fileTargetPath += Path.GetExtension(fileCurrentPath);
                    File.Move(fileCurrentPath, fileTargetPath);

                    rtn.Result = _mapper.Map<FmFileDTO>(newFileModel);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "add",
                        Id = rtn.Result.Id,
                        UserId = args.UserId,
                        Name = rtn.Result.Title,
                        Data = (GetFmTreeFileItem(rtn.Result.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileAddModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "AddFile"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.AddFile.After")
                        .EventHandler<FmFileDTO, IAfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileAddModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<FmFileDTO> UpdateFile(ServiceParamsWithIdentifier<FmFileUpdateModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileUpdateModel>> afterEventParameterModel = null;
                IReturnModel<FmFileDTO> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmFile getFileData = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.UpdateFile.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmFileUpdateModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.UpdateFile_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getFileData = _fmFileRepository.Find(i => i.Id == args.Param.Id);
                    if (getFileData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.UpdateFile_DataNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    int checkTitle = _fmFileRepository.Count(i => i.DirectoryId == getFileData.DirectoryId && i.Title == args.Param.Title && i.Id != getFileData.Id);
                    if (checkTitle > 0)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.UpdateFile_TitleExists);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getFileData.Title = args.Param.Title;
                    getFileData.Description = args.Param.Description;
                    getFileData = _fmFileRepository.Update(getFileData);
                    _fmFileRepository.Save();

                    rtn.Result = _mapper.Map<FmFileDTO>(getFileData);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = rtn.Result.Id,
                        UserId = args.UserId,
                        Name = rtn.Result.Title,
                        Data = (GetFmTreeFileItem(rtn.Result.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileUpdateModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "UpdateFile"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.UpdateFile.After")
                        .EventHandler<FmFileDTO, IAfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileUpdateModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<bool> ChangeStatusFile(ServiceParamsWithIdentifier<ChangeStatusModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<ChangeStatusModel>> afterEventParameterModel = null;
                IReturnModel<bool> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmFile getData = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ChangeStatusFile.Before").EventHandler<bool, ServiceParamsWithIdentifier<ChangeStatusModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ChangeStatusFile_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getData = _fmFileRepository.Find(i => i.Id == args.Param.Id);
                    if (getData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ChangeStatusFile_DataNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getData.Status = args.Param.Status;
                    getData.UpdatingDate = DateTime.Now;
                    getData.UpdatingUserId = args.UserId;
                    getData = _fmFileRepository.Update(getData);
                    _fmFileRepository.Save();
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "remove",
                        Id = args.Param.Id,
                        UserId = args.UserId,
                        Name = getData.Title,
                        Data = (GetFmTreeFileItem(args.Param.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                rtn.Result = cnt;

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<ChangeStatusModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "ChangeStatusFile"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ChangeStatusFile.After")
                        .EventHandler<bool, IAfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<ChangeStatusModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;
                getData = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<FmFileDTO> MoveFile(ServiceParamsWithIdentifier<FmFileMoveModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileMoveModel>> afterEventParameterModel = null;
                IReturnModel<FmFileDTO> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmFile getFileData = null;
                FmDirectory getDirectoryData = null;
                int cacheFileDirectory = 0;
                FmDirectory getOldDirectoryData = null;
                string newFilePath = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.MoveFile.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmFileMoveModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.MoveFile_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getFileData = _fmFileRepository.Find(i => i.Id == args.Param.FileId);
                    if (getFileData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.MoveFile_FileNotFound);
                        cnt = false;
                    }

                    cacheFileDirectory = getFileData.DirectoryId;
                }

                if (cnt)
                {
                    getDirectoryData = _fmDirectoryRepository.Find(i => i.Id == args.Param.TargetDirectoryId);
                    if (getDirectoryData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.MoveFile_DirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    int checkTitle = _fmFileRepository.Count(i => i.DirectoryId == args.Param.TargetDirectoryId && i.Title == getFileData.Title);
                    if (checkTitle > 0)
                    {
                        getFileData.Title += " (2)";
                    }

                    newFilePath = getDirectoryData.Path + "/" + getFileData.Id + Path.GetExtension(getFileData.Path);

                    File.Move(Path.Combine(_storageRootPath, getFileData.Path), Path.Combine(_storageRootPath, newFilePath));

                    getFileData.DirectoryId = args.Param.TargetDirectoryId;
                    getFileData.Path = newFilePath;
                    getFileData.UpdatingDate = DateTime.Now;
                    getFileData.UpdatingUserId = args.UserId;
                    getFileData = _fmFileRepository.Update(getFileData);
                    _fmFileRepository.Save();

                    rtn.Result = _mapper.Map<FmFileDTO>(getFileData);
                }

                if (cnt)
                {
                    getOldDirectoryData = _fmDirectoryRepository.Find(i => i.Id == cacheFileDirectory);
                    if (getOldDirectoryData != null)
                    {
                        _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                        {
                            Key = "Project_Fm",
                            ProcessType = "update",
                            Id = getOldDirectoryData.Id,
                            UserId = args.UserId,
                            Name = getOldDirectoryData.Title,
                            Data = (GetFmTreeDirectoryItem(getOldDirectoryData.Id)).Result
                        });
                    }

                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getDirectoryData.Id,
                        UserId = args.UserId,
                        Name = getDirectoryData.Title,
                        Data = (GetFmTreeDirectoryItem(getDirectoryData.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileMoveModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "MoveFile"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.MoveFile.After")
                        .EventHandler<FmFileDTO, IAfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileMoveModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<FmFileDTO> CopyFile(ServiceParamsWithIdentifier<FmFileCopyModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileCopyModel>> afterEventParameterModel = null;
                IReturnModel<FmFileDTO> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmFile getFileData = null;
                FmDirectory getDirectoryData = null;
                string newFilePath = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.CopyFile.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmFileCopyModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.CopyFile_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getFileData = _fmFileRepository.Find(i => i.Id == args.Param.FileId);
                    if (getFileData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.CopyFile_FileNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getDirectoryData = _fmDirectoryRepository.Find(i => i.Id == args.Param.TargetDirectoryId);
                    if (getDirectoryData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.CopyFile_DirectoryNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    int checkTitle = _fmFileRepository.Count(i => i.DirectoryId == args.Param.TargetDirectoryId && i.Title == getFileData.Title);

                    FmFile newFileModel = new FmFile
                    {
                        UserId = getFileData.UserId,
                        DirectoryId = args.Param.TargetDirectoryId,
                        Title = checkTitle > 0 ? getFileData.Title + " (2)" : getFileData.Title,
                        Description = getFileData.Description,
                        FsKey = Guid.NewGuid(),
                        Path = "/",
                        Size = getFileData.Size,
                        Status = getFileData.Status,
                        AddingDate = DateTime.Now,
                        UpdatingDate = DateTime.Now,
                        AddingUserId = args.UserId,
                        UpdatingUserId = args.UserId
                    };

                    newFileModel = _fmFileRepository.Add(newFileModel);
                    _fmFileRepository.Save();

                    newFilePath = getDirectoryData.Path + "/" + newFileModel.Id + Path.GetExtension(getFileData.Path);

                    File.Copy(Path.Combine(_storageRootPath, getFileData.Path), Path.Combine(_storageRootPath, newFilePath));

                    newFileModel.Path = newFilePath;
                    newFileModel = _fmFileRepository.Update(newFileModel);
                    _fmFileRepository.Save();

                    rtn.Result = _mapper.Map<FmFileDTO>(newFileModel);
                }

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getDirectoryData.Id,
                        UserId = args.UserId,
                        Name = getDirectoryData.Title,
                        Data = (GetFmTreeDirectoryItem(getDirectoryData.Id)).Result
                    });

                    _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
                    {
                        Key = "Project_FmShare",
                        ProcessType = "update",
                        Id = 0,
                        UserId = args.UserId,
                        Name = string.Empty,
                        Data = true
                    });
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileCopyModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "CopyFile"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.CopyFile.After")
                        .EventHandler<FmFileDTO, IAfterEventParameterModel<IReturnModel<FmFileDTO>, ServiceParamsWithIdentifier<FmFileCopyModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<bool> ShareFile(ServiceParamsWithIdentifier<FmFileShareModel> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<FmFileShareModel>> afterEventParameterModel = null;
                IReturnModel<bool> afterEventHandler = null;
                List<ValidationResult> modelValidation = null;
                FmFile getData = null;
                ICollection<FmShare> getSharedData = null;
                List<string> splitUsers = new List<string>();
                List<string> splitGroups = new List<string>();
                FmShare newShareData = null;
                List<int> shareUpUsers = new List<int>();
                List<int> shareUpGroups = new List<int>();
                List<int> shareDownUsers = new List<int>();
                List<int> shareDownGroups = new List<int>();
                List<int> upUsers = null;
                List<int> downUsers = null;
                ICollection<UserInGroup> getGroupsInUsers = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ShareFile.Before").EventHandler<bool, ServiceParamsWithIdentifier<FmFileShareModel>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    modelValidation = args.Param.ModelValidation();

                    if (modelValidation.Any())
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ShareFile_ModelValidationFail);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getData = _fmFileRepository.Find(i => i.Id == args.Param.FileId);
                    if (getData == null)
                    {
                        rtn = rtn.SendError(FmErrorsEnum.ShareFile_DataNotFound);
                        cnt = false;
                    }
                }

                if (cnt)
                {
                    getSharedData = _fmShareRepository.FindAll(i => i.FileId == args.Param.FileId);
                    if (!string.IsNullOrWhiteSpace(args.Param.SharedUserIds))
                    {
                        splitUsers = args.Param.SharedUserIds.Trim().Split(",").ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(args.Param.SharedGroupIds))
                    {
                        splitGroups = args.Param.SharedGroupIds.Trim().Split(",").ToList();
                    }

                    if (getSharedData.Any())
                    {
                        foreach (FmShare data in getSharedData)
                        {
                            if (data.SharedUserId.ToInt(0) > 0)
                            {
                                if (!splitUsers.Any(i => i.ToInt(0) == data.SharedUserId.ToInt(0)))
                                {
                                    _fmShareRepository.Delete(data);
                                    shareDownUsers.Add(data.SharedUserId.ToInt(0));
                                }
                            }

                            if (data.SharedGroupId.ToInt(0) > 0)
                            {
                                if (!splitGroups.Any(i => i.ToInt(0) == data.SharedGroupId.ToInt(0)))
                                {
                                    _fmShareRepository.Delete(data);
                                    shareDownGroups.Add(data.SharedGroupId.ToInt(0));
                                }
                            }
                        }

                        _fmShareRepository.Save();
                    }

                    if (splitUsers.Any())
                    {
                        foreach (string item in splitUsers)
                        {
                            if (_fmShareRepository.Count(i => i.FileId == args.Param.FileId && i.SharedUserId == item.ToInt(0)) == 0)
                            {
                                newShareData = new FmShare
                                {
                                    FileId = getData.Id,
                                    SharedUserId = item.ToInt(0),
                                    Status = true,
                                    AddingDate = DateTime.Now,
                                    UpdatingDate = DateTime.Now,
                                    AddingUserId = args.UserId,
                                    UpdatingUserId = args.UserId
                                };
                                _fmShareRepository.Add(newShareData);
                                shareUpUsers.Add(item.ToInt(0));
                            }
                        }

                        _fmShareRepository.Save();
                    }

                    if (splitGroups.Any())
                    {
                        foreach (string item in splitGroups)
                        {
                            if (_fmShareRepository.Count(i => i.FileId == args.Param.FileId && i.SharedGroupId == item.ToInt(0)) == 0)
                            {
                                newShareData = new FmShare
                                {
                                    FileId = getData.Id,
                                    SharedGroupId = item.ToInt(0),
                                    Status = true,
                                    AddingDate = DateTime.Now,
                                    UpdatingDate = DateTime.Now,
                                    AddingUserId = args.UserId,
                                    UpdatingUserId = args.UserId
                                };
                                _fmShareRepository.Add(newShareData);
                                shareUpGroups.Add(item.ToInt(0));
                            }
                        }

                        _fmShareRepository.Save();
                    }
                }

                if (cnt)
                {
                    upUsers = new List<int>();
                    downUsers = new List<int>();

                    if (shareUpUsers.Any())
                    {
                        upUsers.AddRange(shareUpUsers);
                    }

                    if (shareDownUsers.Any())
                    {
                        downUsers.AddRange(shareDownUsers);
                    }

                    if (shareUpGroups.Any())
                    {
                        foreach (int data in shareUpGroups)
                        {
                            getGroupsInUsers = _userInGroupRepository.FindAll(i => i.GroupId == data);
                            if (getGroupsInUsers.Any())
                            {
                                upUsers.AddRange(getGroupsInUsers.Select(i => i.UserId));
                            }
                        }
                    }

                    if (shareDownGroups.Any())
                    {
                        foreach (int data in shareDownGroups)
                        {
                            getGroupsInUsers = _userInGroupRepository.FindAll(i => i.GroupId == data);
                            if (getGroupsInUsers.Any())
                            {
                                downUsers.AddRange(getGroupsInUsers.Select(i => i.UserId));
                            }
                        }
                    }

                    if (upUsers.Any())
                    {
                        upUsers.Remove(getData.UserId);

                        foreach (int user in upUsers.Distinct())
                        {
                            NotifyShareUpForUser(getData.Id, getData.Title, user, args.UserId);
                        }
                    }

                    if (downUsers.Any())
                    {
                        downUsers.Remove(getData.UserId);

                        foreach (int user in downUsers.Distinct())
                        {
                            NotifyShareDownForUser(getData.Id, getData.Title, user, args.UserId);
                        }
                    }
                }

                //if (cnt && shareUpUsers.Any())
                //{
                //    foreach (var userId in shareUpUsers)
                //        NotifyShareUpForUser(getData.Id, getData.Title, args.UserId, userId);
                //}

                //if (cnt && shareUpGroups.Any())
                //    NotifyShareUpForGroup(getData.Id, getData.Title, shareUpGroups);

                //if (cnt && shareDownUsers.Any())
                //{
                //    foreach (var userId in shareDownUsers)
                //        NotifyShareDownForUser(getData.Id, getData.Title, userId);
                //}

                //if (cnt && shareDownGroups.Any())
                //    NotifyShareDownForGroup(getData.Id, getData.Title, shareDownGroups);

                if (cnt)
                {
                    _hubSyncDataService.OnSync(new HubSyncDataModel<FmTreeItemModel>
                    {
                        Key = "Project_Fm",
                        ProcessType = "update",
                        Id = getData.Id,
                        UserId = args.UserId,
                        Name = getData.Title,
                        Data = (GetFmTreeDirectoryItem(getData.Id)).Result
                    });
                }

                rtn.Result = cnt;

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<FmFileShareModel>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "ShareFile"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.ShareFile.After")
                        .EventHandler<bool, IAfterEventParameterModel<IReturnModel<bool>, ServiceParamsWithIdentifier<FmFileShareModel>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;
                modelValidation = null;
                getData = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<IList<FmTreeItemModel>> FmTreeItems(ServiceParamsWithIdentifier<int> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<IList<FmTreeItemModel>> rtn = new ReturnModel<IList<FmTreeItemModel>>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<IList<FmTreeItemModel>>, ServiceParamsWithIdentifier<int>> afterEventParameterModel = null;
                IReturnModel<IList<FmTreeItemModel>> afterEventHandler = null;
                ICollection<FmDirectory> getDirectories = null;
                IReturnModel<FmTreeItemModel> getDirectoryTreeItem = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.FmTreeItems.Before").EventHandler<bool, ServiceParamsWithIdentifier<int>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    rtn.Result = new List<FmTreeItemModel>();

                    getDirectories = _fmDirectoryRepository.FindAll(i => i.UserId == args.Param && i.ParentDirectoryId == null && i.Status == true);
                    if (getDirectories.Any())
                    {
                        foreach (FmDirectory directory in getDirectories.OrderBy(i => i.Title))
                        {
                            if (!cnt)
                            {
                                continue;
                            }

                            getDirectoryTreeItem = GetFmTreeDirectoryItem(directory.Id);
                            if (getDirectoryTreeItem.Error.Status)
                            {
                                rtn.Error = getDirectoryTreeItem.Error;
                                cnt = false;
                            }
                            else
                            {
                                rtn.Result.Add(getDirectoryTreeItem.Result);
                            }
                        }
                    }
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<IList<FmTreeItemModel>>, ServiceParamsWithIdentifier<int>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "FmTreeItems"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.FmTreeItems.After")
                        .EventHandler<IList<FmTreeItemModel>, IAfterEventParameterModel<IReturnModel<IList<FmTreeItemModel>>, ServiceParamsWithIdentifier<int>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        public IReturnModel<IList<FmShareFileModel>> FmSharedFiles(ServiceParamsWithIdentifier<int> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            IReturnModel<IList<FmShareFileModel>> rtn = new ReturnModel<IList<FmShareFileModel>>(_logger);

            try
            {
                #region Variables

                bool cnt = true;
                IReturnModel<bool> beforeEventHandler = null;
                AfterEventParameterModel<IReturnModel<IList<FmShareFileModel>>, ServiceParamsWithIdentifier<int>> afterEventParameterModel = null;
                IReturnModel<IList<FmShareFileModel>> afterEventHandler = null;
                ICollection<UserInGroup> userInGroups = null;
                ICollection<FmShare> shareData = null;
                List<int> userInGroupIds = null;
                List<int> sharedFiles = null;
                IEnumerable<FmShare> filterOnlyFileSharedData = null;
                IEnumerable<FmShare> filterOnlyDirectorySharedData = null;
                ICollection<FmDirectory> getSharedDirectoryData = null;
                ICollection<FmFile> findSharedDirectoryInFiles = null;
                IEnumerable<int> clearFileIdList = null;
                ICollection<FmFile> getsharedFilesData = null;
                IList<FmShareFileModel> cloneResult = null;

                #endregion Variables

                #region Before Event Handler

                beforeEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.FmSharedFiles.Before").EventHandler<bool, ServiceParamsWithIdentifier<int>>(args);
                if (beforeEventHandler != null)
                {
                    if (beforeEventHandler.Error.Status)
                    {
                        rtn.Error = beforeEventHandler.Error;
                        cnt = false;
                    }
                }

                #endregion Before Event Handler

                #region Action Body

                if (cnt)
                {
                    rtn.Result = new List<FmShareFileModel>();

                    userInGroups = _userInGroupRepository.FindAll(i => i.UserId == args.Param);
                    userInGroupIds = userInGroups.Select(i => i.GroupId).ToList();
                    if (userInGroups.Any())
                    {
                        shareData = _fmShareRepository.FindAll(i => i.SharedGroupId > 0 && userInGroupIds.Contains((int)i.SharedGroupId) || i.SharedUserId == args.Param);
                    }
                    else
                    {
                        shareData = _fmShareRepository.FindAll(i => i.SharedUserId == args.Param);
                    }

                    sharedFiles = new List<int>();

                    filterOnlyFileSharedData = shareData.Where(i => i.FileId != null);
                    if (filterOnlyFileSharedData.Any())
                    {
                        sharedFiles.AddRange(filterOnlyFileSharedData.Select(i => i.FileId.ToInt(0)));
                    }

                    filterOnlyDirectorySharedData = shareData.Where(i => i.DirectoryId != null);
                    if (filterOnlyDirectorySharedData.Any())
                    {
                        foreach (int? sharedDirectory in filterOnlyDirectorySharedData.Select(i => i.DirectoryId))
                        {
                            getSharedDirectoryData = _fmDirectoryRepository.FindAll(i => i.Id == sharedDirectory);
                            if (getSharedDirectoryData != null)
                            {
                                foreach (FmDirectory dataItem in getSharedDirectoryData)
                                {
                                    findSharedDirectoryInFiles = _fmFileRepository.FindAll(i => i.Path.StartsWith(dataItem.Path));
                                    if (findSharedDirectoryInFiles.Any())
                                    {
                                        sharedFiles.AddRange(findSharedDirectoryInFiles.Select(i => i.Id));
                                    }
                                }
                            }
                        }
                    }

                    clearFileIdList = sharedFiles.Distinct();

                    getsharedFilesData = _fmFileRepository.FindAll(i => clearFileIdList.Contains(i.Id) && i.Id != args.Param);

                    foreach (FmFile item in getsharedFilesData)
                    {
                        rtn.Result.Add((GetShareFileItem(item.Id, item.UserId, args.Param)).Result);
                    }

                    cloneResult = rtn.Result;

                    if (cloneResult.Any())
                    {
                        foreach (FmShareFileModel file in cloneResult)
                        {
                            FmFile getSharedFileData = _fmFileRepository.Find(i => i.Id == file.FileId);
                            if (!getSharedFileData.Status || getSharedFileData.UserId == args.Param)
                            {
                                cloneResult.Remove(file);
                            }
                        }
                    }

                    rtn.Result = cloneResult;
                }

                #endregion Action Body

                #region After Event Handler

                if (cnt)
                {
                    afterEventParameterModel = new AfterEventParameterModel<IReturnModel<IList<FmShareFileModel>>, ServiceParamsWithIdentifier<int>>
                    {
                        DataToBeSent = rtn,
                        ActionParameter = args,
                        ModuleName = _moduleName,
                        ServiceName = _serviceName,
                        ActionName = "FmSharedFiles"
                    };
                    afterEventHandler = _eventService.GetEvent(_moduleName, $"{_serviceName}.FmSharedFiles.After")
                        .EventHandler<IList<FmShareFileModel>, IAfterEventParameterModel<IReturnModel<IList<FmShareFileModel>>, ServiceParamsWithIdentifier<int>>>(afterEventParameterModel);
                    if (afterEventHandler != null)
                    {
                        if (afterEventHandler.Error.Status)
                        {
                            rtn.Error = afterEventHandler.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result = afterEventHandler.Result;
                        }
                    }
                }

                #endregion After Event Handler

                #region Clear Memory

                args = null;
                beforeEventHandler = null;
                afterEventParameterModel = null;
                afterEventHandler = null;

                #endregion Clear Memory
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            return rtn;
        }

        #endregion Actions

        #region Helpers

        private bool ChangeStatusFmDirectoryDeep(int parentId, int userId, bool status)
        {
            #region Variables

            ICollection<FmDirectory> getDirectories = null;
            ICollection<FmFile> getFiles = null;

            #endregion Variables

            #region Action Body

            getDirectories = _fmDirectoryRepository.FindAll(i => i.ParentDirectoryId == parentId);
            if (getDirectories.Any())
            {
                foreach (FmDirectory directory in getDirectories)
                {
                    ChangeStatusFmDirectoryDeep(directory.Id, userId, status);

                    directory.Status = status;
                    directory.UpdatingDate = DateTime.Now;
                    directory.UpdatingUserId = userId;
                    _fmDirectoryRepository.Update(directory);

                    getFiles = _fmFileRepository.FindAll(i => i.DirectoryId == directory.Id);
                    if (getFiles.Any())
                    {
                        foreach (FmFile file in getFiles)
                        {
                            file.Status = false;
                            file.UpdatingDate = DateTime.Now;
                            file.UpdatingUserId = userId;
                            _fmFileRepository.Update(file);
                        }
                    }
                }

                _fmDirectoryRepository.Save();
            }

            #endregion Action Body

            #region Clear Memory

            getDirectories = null;
            getFiles = null;

            #endregion Clear Memory

            return true;
        }

        private bool ChangePathDirectoryDeep(string currentPath, int directoryId, int userId)
        {
            #region Variables

            ICollection<FmDirectory> getChildDirectories = null;
            ICollection<FmFile> getFiles = null;

            #endregion Variables

            #region Action Body

            getFiles = _fmFileRepository.FindAll(i => i.DirectoryId == directoryId);
            if (getFiles.Any())
            {
                foreach (FmFile file in getFiles)
                {
                    file.Path = currentPath + "/" + file.Id + Path.GetExtension(file.Path);
                    file.UpdatingDate = DateTime.Now;
                    file.UpdatingUserId = userId;
                    _fmFileRepository.Update(file);
                }

                _fmFileRepository.Save();
            }

            getChildDirectories = _fmDirectoryRepository.FindAll(i => i.ParentDirectoryId == directoryId);
            if (getChildDirectories.Any())
            {
                foreach (FmDirectory childDirectory in getChildDirectories)
                {
                    childDirectory.Path = currentPath + "/" + childDirectory.Id;
                    childDirectory.UpdatingDate = DateTime.Now;
                    childDirectory.UpdatingUserId = userId;
                    _fmDirectoryRepository.Update(childDirectory);
                    _fmDirectoryRepository.Save();

                    ChangePathDirectoryDeep(childDirectory.Path, childDirectory.Id, userId);
                }
            }

            #endregion Action Body

            #region Clear Memory

            getChildDirectories = null;
            getFiles = null;

            #endregion Clear Memory

            return true;
        }

        private bool DirectoryCopyData(ICollection<FmDirectory> dataListCache, int newDirectoryId, int copiedDirectoryId, string currentPath, int userId)
        {
            #region Variables

            IEnumerable<FmDirectory> getChildDirectories = null;
            FmDirectory newDirectoryData = null;

            #endregion Variables

            #region Action Body

            FileCopyData(copiedDirectoryId, newDirectoryId, currentPath, userId);

            getChildDirectories = dataListCache.Where(i => i.ParentDirectoryId == copiedDirectoryId);
            if (getChildDirectories.Any())
            {
                foreach (FmDirectory directory in getChildDirectories)
                {
                    newDirectoryData = new FmDirectory
                    {
                        UserId = directory.UserId,
                        ParentDirectoryId = newDirectoryId,
                        Title = directory.Title,
                        Description = directory.Description,
                        Path = "/",
                        FsKey = Guid.NewGuid(),
                        Size = directory.Size,
                        FileCount = directory.FileCount,
                        Status = directory.Status,
                        AddingDate = DateTime.Now,
                        UpdatingDate = DateTime.Now,
                        AddingUserId = userId,
                        UpdatingUserId = userId
                    };
                    newDirectoryData = _fmDirectoryRepository.Add(newDirectoryData);
                    _fmDirectoryRepository.Save();

                    newDirectoryData.Path = currentPath + "/" + newDirectoryData.Id;
                    newDirectoryData = _fmDirectoryRepository.Update(newDirectoryData);
                    _fmDirectoryRepository.Save();

                    Directory.CreateDirectory(Path.Combine(_storageRootPath, newDirectoryData.Path));
                    DirectoryCopyData(dataListCache, newDirectoryData.Id, directory.Id, newDirectoryData.Path, userId);
                }
            }

            #endregion Action Body

            #region Clear Memory

            getChildDirectories = null;
            newDirectoryData = null;

            #endregion Clear Memory

            return true;
        }

        private bool FileCopyData(int copiedDirectoryId, int newDirectoryId, string currentDirectoryPath, int userId)
        {
            #region Variables

            ICollection<FmFile> files = null;
            FmFile newFile = null;

            #endregion Variables

            #region Action Body

            files = _fmFileRepository.FindAll(i => i.DirectoryId == copiedDirectoryId);
            if (files.Any())
            {
                foreach (FmFile file in files)
                {
                    newFile = new FmFile
                    {
                        UserId = file.UserId,
                        DirectoryId = newDirectoryId,
                        Title = file.Title,
                        Description = file.Description,
                        FsKey = Guid.NewGuid(),
                        Path = "/",
                        Size = file.Size,
                        Status = file.Status,
                        AddingDate = DateTime.Now,
                        UpdatingDate = DateTime.Now,
                        AddingUserId = userId,
                        UpdatingUserId = userId
                    };

                    newFile = _fmFileRepository.Add(newFile);
                    _fmFileRepository.Save();

                    newFile.Path = currentDirectoryPath + "/" + newFile.Id + Path.GetExtension(file.Path);
                    newFile = _fmFileRepository.Update(newFile);
                    _fmFileRepository.Save();

                    File.Copy(Path.Combine(_storageRootPath, file.Path), Path.Combine(_storageRootPath, newFile.Path));
                }
            }

            #endregion Action Body

            #region Clear Memory

            files = null;
            newFile = null;

            #endregion Clear Memory

            return true;
        }

        private IReturnModel<FmTreeItemModel> GetFmTreeDirectoryItem(int directoryId)
        {
            #region Variables

            IReturnModel<FmTreeItemModel> rtn = new ReturnModel<FmTreeItemModel>(_logger);
            bool cnt = true;
            FmDirectory getDirectory = null;
            ICollection<FmDirectory> children = null;
            IReturnModel<FmTreeItemModel> getChildItem = null;
            ICollection<FmFile> getFiles = null;
            IReturnModel<FmTreeItemModel> getFileTreeItem = null;
            ICollection<FmShare> getSharedData = null;

            #endregion Variables

            #region Action Body

            getDirectory = _fmDirectoryRepository.Find(i => i.Id == directoryId);
            if (getDirectory == null)
            {
                rtn = rtn.SendError(FmErrorsEnum.GetFmTreeDirectoryItem_DirectoryNotFound);
                cnt = false;
            }

            if (cnt)
            {
                rtn.Result = new FmTreeItemModel
                {
                    Value = getDirectory.Id,
                    Text = getDirectory.Title,
                    ItemType = "directory",
                    DirectoryData = _mapper.Map<FmDirectoryDTO>(getDirectory),
                    FileData = null,
                    FullDirectoryPath = GetDirectoryFullPath(getDirectory.Path),
                    Shared = new FmTreeItemSharedModel()
                };

                children = _fmDirectoryRepository.FindAll(i => i.ParentDirectoryId == getDirectory.Id && i.Status == true);
                if (children.Any())
                {
                    foreach (FmDirectory child in children.OrderBy(i => i.Title))
                    {
                        if (!cnt)
                        {
                            continue;
                        }

                        getChildItem = GetFmTreeDirectoryItem(child.Id);
                        if (getChildItem.Error.Status)
                        {
                            rtn.Error = getChildItem.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result.Children.Add(getChildItem.Result);
                        }
                    }
                }

                getFiles = _fmFileRepository.FindAll(i => i.DirectoryId == directoryId && i.Status == true);
                if (getFiles.Any())
                {
                    foreach (FmFile file in getFiles.OrderBy(i => i.Title))
                    {
                        if (!cnt)
                        {
                            continue;
                        }

                        getFileTreeItem = GetFmTreeFileItem(file.Id);
                        if (getFileTreeItem.Error.Status)
                        {
                            rtn.Error = getFileTreeItem.Error;
                            cnt = false;
                        }
                        else
                        {
                            rtn.Result.Children.Add(getFileTreeItem.Result);
                        }
                    }
                }

                getSharedData = _fmShareRepository.FindAll(i => i.DirectoryId == getDirectory.Id);
                if (getSharedData.Any())
                {
                    foreach (FmShare data in getSharedData)
                    {
                        if (data.SharedGroupId.ToInt(0) > 0)
                        {
                            rtn.Result.Shared.Groups.Add(_mapper.Map<FmShareDTO>(data));
                        }

                        if (data.SharedUserId.ToInt(0) > 0)
                        {
                            rtn.Result.Shared.Users.Add(_mapper.Map<FmShareDTO>(data));
                        }
                    }
                }
            }

            #endregion Action Body

            #region Clear Memory

            getDirectory = null;
            children = null;
            getChildItem = null;
            getFiles = null;
            getFileTreeItem = null;

            #endregion Clear Memory

            return rtn;
        }

        private IReturnModel<FmTreeItemModel> GetFmTreeFileItem(int fileId)
        {
            #region Variables

            IReturnModel<FmTreeItemModel> rtn = new ReturnModel<FmTreeItemModel>(_logger);
            bool cnt = true;
            FmFile getFileData = null;
            ICollection<FmShare> getSharedData = null;

            #endregion Variables

            #region Action Body

            getFileData = _fmFileRepository.Find(i => i.Id == fileId);
            if (getFileData == null)
            {
                rtn = rtn.SendError(FmErrorsEnum.GetFmTreeDirectoryItem_DirectoryNotFound);
                cnt = false;
            }

            if (cnt)
            {
                rtn.Result = new FmTreeItemModel
                {
                    Value = getFileData.Id,
                    Text = getFileData.Title,
                    ItemType = "file",
                    DirectoryData = null,
                    FileData = _mapper.Map<FmFileDTO>(getFileData),
                    FullDirectoryPath = GetFileFullPath(getFileData.Path),
                    Shared = new FmTreeItemSharedModel()
                };

                getSharedData = _fmShareRepository.FindAll(i => i.FileId == getFileData.Id);
                if (getSharedData.Any())
                {
                    foreach (FmShare data in getSharedData)
                    {
                        if (data.SharedGroupId.ToInt(0) > 0)
                        {
                            rtn.Result.Shared.Groups.Add(_mapper.Map<FmShareDTO>(data));
                        }

                        if (data.SharedUserId.ToInt(0) > 0)
                        {
                            rtn.Result.Shared.Users.Add(_mapper.Map<FmShareDTO>(data));
                        }
                    }
                }
            }

            #endregion Action Body

            #region Clear Memory

            getFileData = null;
            getSharedData = null;

            #endregion Clear Memory

            return rtn;
        }


        private IReturnModel<FmShareFileModel> GetShareFileItem(int fileId, int ownerUserId, int sharedUserId)
        {
            #region Variables

            IReturnModel<FmShareFileModel> rtn = new ReturnModel<FmShareFileModel>(_logger);
            bool cnt = true;
            FmFile getFile = null;
            FmDirectory getDirectory = null;
            string fileFullPath = null;
            string[] splitFilePath = null;
            FmDirectory getPathDirectory = null;

            #endregion Variables

            #region Action Body

            getFile = _fmFileRepository.Find(i => i.Id == fileId);
            if (getFile == null)
            {
                rtn = rtn.SendError(FmErrorsEnum.GetShareFileItem_FileNotFound);
                cnt = false;
            }

            if (cnt)
            {
                getDirectory = _fmDirectoryRepository.Find(i => i.Id == getFile.DirectoryId);
                if (getDirectory == null)
                {
                    rtn = rtn.SendError(FmErrorsEnum.GetShareFileItem_DirectoryNotFound);
                    cnt = false;
                }
            }

            if (cnt)
            {
                fileFullPath = string.Empty;
                splitFilePath = getFile.Path.Split("/");
                if (splitFilePath.Length > 0)
                {
                    for (int index = 0; index < (splitFilePath.Length - 1); index++)
                    {
                        getPathDirectory = _fmDirectoryRepository.Find(i => i.Id == splitFilePath[index].ToInt(0));
                        if (getPathDirectory != null)
                        {
                            fileFullPath = string.IsNullOrWhiteSpace(fileFullPath) ? getPathDirectory.Title : getPathDirectory.Title + "/" + fileFullPath;
                        }
                    }
                }

                fileFullPath += "/" + getFile.Title;
            }

            if (cnt)
            {
                rtn.Result = new FmShareFileModel
                {
                    FileId = getFile.Id,
                    OwnerUserId = ownerUserId,
                    SharedUserId = sharedUserId,
                    FileTitle = getFile.Title,
                    FileDescription = getFile.Description,
                    FileSize = getFile.Size,
                    FileFullPath = fileFullPath,
                    DirectoryTitle = getDirectory.Title,
                    FilePath = getFile.Path
                };
            }

            #endregion Action Body

            #region Clear Memory

            getFile = null;
            getDirectory = null;
            fileFullPath = null;
            splitFilePath = null;
            getPathDirectory = null;

            #endregion Clear Memory

            return rtn;
        }

        private bool NotifyShareUpForUser(int id, string title, int userId, int ownerUserId)
        {
            _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
            {
                Key = "Project_FmShare",
                ProcessType = "add",
                Id = id,
                UserId = ownerUserId,
                Name = title,
                Data = true
            }, "user_" + userId);

            return true;
        }

        private bool NotifyShareDownForUser(int id, string title, int userId, int ownerUserId)
        {
            _hubSyncDataService.OnSync(new HubSyncDataModel<bool>
            {
                Key = "Project_FmShare",
                ProcessType = "remove",
                Id = id,
                UserId = ownerUserId,
                Name = title,
                Data = true
            }, "user_" + userId);

            return true;
        }

        private string GetDirectoryFullPath(string path)
        {
            string rtn = string.Empty;
            string[] splitPath = path.Split("/");
            foreach (string splitPathItem in splitPath)
            {
                FmDirectory getDirectoryData = _fmDirectoryRepository.Find(i => i.Id == splitPathItem.Trim().ToInt(0));
                if (getDirectoryData != null)
                {
                    rtn = string.IsNullOrEmpty(rtn) ? getDirectoryData.Title : rtn + " / " + getDirectoryData.Title;
                }
                getDirectoryData = null;
            }

            return rtn;
        }

        private string GetFileFullPath(string path)
        {
            string[] splitPath = path.Split("/");
            string[] directoryPathArray = splitPath;
            Array.Resize(ref directoryPathArray, directoryPathArray.Length - 1);

            string directoryPath = GetDirectoryFullPath(string.Join("/", directoryPathArray));
            int index = splitPath.Length - 1;
            FmFile getFile = _fmFileRepository.Find(i => i.Id == Path.GetFileNameWithoutExtension(splitPath[index]).ToInt(0));
            if (getFile != null)
            {
                directoryPath += " / " + getFile.Title;
            }

            splitPath = null;
            directoryPathArray = null;
            getFile = null;

            return directoryPath;
        }

        #endregion
    }
}
