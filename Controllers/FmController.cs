using CSBEF.Core.Concretes;
using CSBEF.Core.Enums;
using CSBEF.Core.Helpers;
using CSBEF.Core.Interfaces;
using CSBEF.Core.Models;
using CSBEF.Module.UserCloudManager.Interfaces.Service;
using CSBEF.Module.UserCloudManager.Models.DTO;
using CSBEF.Module.UserCloudManager.Models.Request;
using CSBEF.Module.UserCloudManager.Models.Return;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CSBEF.Module.UserCloudManager.Controllers
{
    [ApiController]
    public class FmController : ControllerBase
    {
        #region Dependencies

        private readonly IConfiguration _configuration;
        private readonly ILogger<ILog> _logger;
        private readonly IFmService _service;

        #endregion Dependencies

        #region Construction

        public FmController(IConfiguration configuration, ILogger<ILog> logger, IFmService service)
        {
            _configuration = configuration;
            _logger = logger;
            _service = service;
        }

        #endregion Construction

        #region 

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/AddDirectory")]
        [HttpPost]
        public ActionResult<IReturnModel<FmDirectoryDTO>> AddDirectory([FromBody]FmDirectoryAddModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<FmDirectoryDTO> rtn = new ReturnModel<FmDirectoryDTO>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:AddDirectory"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmDirectoryAddModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmDirectoryAddModel>(filter, userId, tokenId);
                serviceFilterModel.Param.UserId = userId;
                IReturnModel<FmDirectoryDTO> serviceAction = _service.AddDirectory(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/UpdateDirectory")]
        [HttpPost]
        public ActionResult<IReturnModel<FmDirectoryDTO>> UpdateDirectory([FromBody]FmDirectoryUpdateModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<FmDirectoryDTO> rtn = new ReturnModel<FmDirectoryDTO>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:UpdateDirectory"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmDirectoryUpdateModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmDirectoryUpdateModel>(filter, userId, tokenId);
                IReturnModel<FmDirectoryDTO> serviceAction = _service.UpdateDirectory(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/ChangeStatusDirectory")]
        [HttpPost]
        public ActionResult<IReturnModel<bool>> ChangeStatusDirectory([FromBody]ChangeStatusModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:ChangeStatusDirectory"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<ChangeStatusModel> serviceFilterModel = new ServiceParamsWithIdentifier<ChangeStatusModel>(filter, userId, tokenId);
                IReturnModel<bool> serviceAction = _service.ChangeStatusDirectory(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/MoveDirectory")]
        [HttpPost]
        public ActionResult<IReturnModel<bool>> MoveDirectory([FromBody]DirectoryMoveModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:MoveDirectory"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<DirectoryMoveModel> serviceFilterModel = new ServiceParamsWithIdentifier<DirectoryMoveModel>(filter, userId, tokenId);
                IReturnModel<bool> serviceAction = _service.MoveDirectory(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/CopyDirectory")]
        [HttpPost]
        public ActionResult<IReturnModel<bool>> CopyDirectory([FromBody]DirectoryCopModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:CopyDirectory"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<DirectoryCopModel> serviceFilterModel = new ServiceParamsWithIdentifier<DirectoryCopModel>(filter, userId, tokenId);
                IReturnModel<bool> serviceAction = _service.CopyDirectory(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/ShareDirectory")]
        [HttpPost]
        public ActionResult<IReturnModel<bool>> ShareDirectory([FromBody]FmDirectoryShareModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:ShareDirectory"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmDirectoryShareModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmDirectoryShareModel>(filter, userId, tokenId);
                IReturnModel<bool> serviceAction = _service.ShareDirectory(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/AddFile")]
        [HttpPost]
        public ActionResult<IReturnModel<FmFileDTO>> AddFile([FromForm]FmFileAddModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:AddFile"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmFileAddModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmFileAddModel>(filter, userId, tokenId);
                serviceFilterModel.Param.UserId = userId;
                IReturnModel<FmFileDTO> serviceAction = _service.AddFile(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/UpdateFile")]
        [HttpPost]
        public ActionResult<IReturnModel<FmFileDTO>> UpdateFile([FromBody]FmFileUpdateModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:UpdateFile"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmFileUpdateModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmFileUpdateModel>(filter, userId, tokenId);
                IReturnModel<FmFileDTO> serviceAction = _service.UpdateFile(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/ChangeStatusFile")]
        [HttpPost]
        public ActionResult<IReturnModel<bool>> ChangeStatusFile([FromBody]ChangeStatusModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:ChangeStatusFile"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<ChangeStatusModel> serviceFilterModel = new ServiceParamsWithIdentifier<ChangeStatusModel>(filter, userId, tokenId);
                IReturnModel<bool> serviceAction = _service.ChangeStatusFile(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/MoveFile")]
        [HttpPost]
        public ActionResult<IReturnModel<FmFileDTO>> MoveFile([FromBody]FmFileMoveModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:MoveFile"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmFileMoveModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmFileMoveModel>(filter, userId, tokenId);
                IReturnModel<FmFileDTO> serviceAction = _service.MoveFile(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/CopyFile")]
        [HttpPost]
        public ActionResult<IReturnModel<FmFileDTO>> CopyFile([FromBody]FmFileCopyModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<FmFileDTO> rtn = new ReturnModel<FmFileDTO>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:CopyFile"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmFileCopyModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmFileCopyModel>(filter, userId, tokenId);
                IReturnModel<FmFileDTO> serviceAction = _service.CopyFile(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Authorize(Roles = "Root, Root.Project, Root.Project.FileManager")]
        [Route("api/UserCloudManager/Fm/ShareFile")]
        [HttpPost]
        public ActionResult<IReturnModel<bool>> ShareFile([FromBody]FmFileShareModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<bool> rtn = new ReturnModel<bool>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:ShareFile"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<FmFileShareModel> serviceFilterModel = new ServiceParamsWithIdentifier<FmFileShareModel>(filter, userId, tokenId);
                IReturnModel<bool> serviceAction = _service.ShareFile(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Route("api/UserCloudManager/Fm/FmTreeItems")]
        [HttpGet]
        public ActionResult<IReturnModel<IList<FmTreeItemModel>>> FmTreeItems([FromQuery]FmTreeItemsModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<IList<FmTreeItemModel>> rtn = new ReturnModel<IList<FmTreeItemModel>>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:FmTreeItems"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<int> serviceFilterModel = new ServiceParamsWithIdentifier<int>(filter.UserId, userId, tokenId);
                IReturnModel<IList<FmTreeItemModel>> serviceAction = _service.FmTreeItems(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        [Route("api/UserCloudManager/Fm/GetSharedFiles")]
        [HttpGet]
        public ActionResult<IReturnModel<IList<FmShareFileModel>>> GetSharedFiles([FromQuery]FmSharedFilesModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            #region Declares

            IReturnModel<IList<FmShareFileModel>> rtn = new ReturnModel<IList<FmShareFileModel>>(_logger);

            #endregion Declares

            #region Hash Control

            if (!filter.HashControl(_configuration["AppSettings:HashSecureKeys:UserCloudManager:Fm:GetSharedFiles"]))
            {
                _logger.LogError("InvalidHash: " + filter.Hash);
                return BadRequest(rtn.SendError(GlobalErrors.HashCodeInValid));
            }

            #endregion Hash Control

            #region Action Body

            try
            {
                int userId = Tools.GetTokenNameClaim(HttpContext);
                int tokenId = Tools.GetTokenIdClaim(HttpContext);
                ServiceParamsWithIdentifier<int> serviceFilterModel = new ServiceParamsWithIdentifier<int>(userId, userId, tokenId);
                IReturnModel<IList<FmShareFileModel>> serviceAction = _service.FmSharedFiles(serviceFilterModel);
                if (serviceAction.Error.Status)
                {
                    rtn.Error = serviceAction.Error;
                }
                else
                {
                    rtn.Result = serviceAction.Result;
                }
            }
            catch (Exception ex)
            {
                rtn = rtn.SendError(GlobalErrors.TechnicalError, ex);
            }

            #endregion Action Body

            return Ok(rtn);
        }

        #endregion 
    }
}
