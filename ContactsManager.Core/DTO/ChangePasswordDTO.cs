using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsManager.Core.DTO
{
 
  public class ChangePasswordDTO
  {
    [Required]
    public string Token { get; set; }

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email value should be a valid email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password Name can't be blank")]
    [DataType(DataType.Password)]
    [DisplayName("New Password")]

    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password Name can't be blank")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Password and Confirm password do't match ")]
    [DisplayName("Confirm Password")]
    public string ConfirmNewPassword { get; set; }
  }
}
