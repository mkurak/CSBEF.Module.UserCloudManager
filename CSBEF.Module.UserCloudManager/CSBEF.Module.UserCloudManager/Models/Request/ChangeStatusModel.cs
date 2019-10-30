using CSBEF.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class ChangeStatusModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_IdRequired")]
        [Range(minimum: 1, maximum: 2147483647, ErrorMessage = "ModelValidationError_IdIsZero")]
        public int Id { get; set; }

        public bool Status { get; set; }
    }
}
