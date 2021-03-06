﻿using CSBEF.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmSharedFilesModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_UserIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_UserIdWrong")]
        public int UserId { get; set; }
    }
}
