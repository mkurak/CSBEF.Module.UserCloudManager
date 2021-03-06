﻿using CSBEF.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmFileShareModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_FileIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_FileIdWrong")]
        public int FileId { get; set; }
        public string SharedUserIds { get; set; }
        public string SharedGroupIds { get; set; }
    }
}
