﻿using CSBEF.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmDirectoryShareModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_DirectoryIdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_DirectoryIdWrong")]
        public int DirectoryId { get; set; }

        public string SharedUserIds { get; set; }

        public string SharedGroupIds { get; set; }
    }
}
