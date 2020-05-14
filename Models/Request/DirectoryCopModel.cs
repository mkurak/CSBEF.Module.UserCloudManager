using CSBEF.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class DirectoryCopModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_CopiedDirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_CopiedDirectoryIdWrong")]
        public int CopiedDirectoryId { get; set; }

        [Required(ErrorMessage = "ModelValidationError_TargetDirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_TargetDirectoryIdWrong")]
        public int TargetDirectoryId { get; set; }
    }
}
