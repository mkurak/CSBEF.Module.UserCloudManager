using CSBEF.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmFileAddModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_UserIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_UserIdWrong")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ModelValidationError_DirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_DirectoryIdWrong")]
        public int DirectoryId { get; set; }

        [Required(ErrorMessage = "ModelValidationError_TitleRequired")]
        [StringLength(maximumLength: 256, ErrorMessage = "ModelValidationError_TitleMaxLen")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "ModelValidationError_FileRequired")]
        public IFormFile File { get; set; }
    }
}
