using ContactsManager.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsManager.Core.DTO
{
  public class RegisterDTO
  {
    [Required(ErrorMessage = "Person Name can't be blank")]

    public string PersonName {  get; set; }

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email value should be a valid email")]
    [DataType(DataType.EmailAddress)]
    [Remote(action: "IsEmailAlreadyRegisteres", controller:"Account",ErrorMessage ="Email is already use")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone Name can't be blank")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number should contain numbers only")]
    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; }


    [Required(ErrorMessage = "Password Name can't be blank")]
    [DataType (DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm Password Name can't be blank")]
    [DataType(DataType.Password)]
    [Compare("Password",ErrorMessage ="Password and Confirm password do't match ")]
    public string ConfirmPassword {  get; set; }
    public UserTypeOption UserType { get; set; } = UserTypeOption.User;

  }
}
