using CSBEF.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmFileMoveModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_FileIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_FileIdWrong")]
        public int FileId { get; set; }

        [Required(ErrorMessage = "ModelValidationError_TargetDirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_TargetDirectoryIdWrong")]
        public int TargetDirectoryId { get; set; }
    }
}
