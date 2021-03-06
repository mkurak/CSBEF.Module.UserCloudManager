﻿using CSBEF.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmTreeItemsModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_UserIdRequired")]
        [Range(minimum: 1, maximum: 2147483647, ErrorMessage = "ModelValidationError_UserIdIsZero")]
        public int UserId { get; set; }
    }
}
