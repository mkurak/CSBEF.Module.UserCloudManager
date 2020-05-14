using CSBEF.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class DirectoryMoveModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_MovedDirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_MovedDirectoryIdWrong")]
        public int MovedDirectoryId { get; set; }

        [Required(ErrorMessage = "ModelValidationError_TargetDirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_TargetDirectoryIdWrong")]
        public int TargetDirectoryId { get; set; }
    }
}
