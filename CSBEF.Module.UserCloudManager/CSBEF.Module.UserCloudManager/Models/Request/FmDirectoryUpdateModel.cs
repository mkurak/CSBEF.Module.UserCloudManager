using CSBEF.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CSBEF.Module.UserCloudManager.Models.Request
{
    public class FmDirectoryUpdateModel : HashControlModel
    {
        [Required(ErrorMessage = "ModelValidationError_IdRequired")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "ModelValidationError_IdWrong")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ModelValidationError_TitleRequired")]
        [StringLength(maximumLength: 256, ErrorMessage = "ModelValidationError_TitleMaxLen")]
        public string Title { get; set; }


        public string Description { get; set; }
    }
}
