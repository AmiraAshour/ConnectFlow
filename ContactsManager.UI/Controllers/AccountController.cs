using ContactsManager.Core.Domain.IdentityEnitities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;
using ContactsManager.Core.ServicesContracts;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Composition;
using System.Security.Claims;

namespace ContactsManager.UI.Controllers
{
  //[AllowAnonymous]
  public class AccountController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly IMemoryCache _cache;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IEmailSender emailSender, IMemoryCache memoryCache)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _emailSender = emailSender;
      _cache = memoryCache;
    }

    #region Register and confirm email
    [HttpGet]
    [Authorize("NotAuthorized")]
    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
    [Authorize("NotAuthorized")]
    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
      if (!ModelState.IsValid)
      {
        var error = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        ViewBag.Errors = error;
        return View(registerDTO);
      }

      ApplicationUser user = new ApplicationUser()
      {
        PersonName = registerDTO.PersonName,
        Email = registerDTO.Email,
        UserName = registerDTO.Email,
        PhoneNumber = registerDTO.Phone
      };

      IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

      if (result.Succeeded)
      {
        if (registerDTO.UserType == UserTypeOption.Admin)
        {
          if (await _roleManager.FindByNameAsync(UserTypeOption.Admin.ToString()) is null)
            await _roleManager.CreateAsync(new ApplicationRole() { Name = UserTypeOption.Admin.ToString() });

          await _userManager.AddToRoleAsync(user, UserTypeOption.Admin.ToString());
        }
        else
        {
          if (await _roleManager.FindByNameAsync(UserTypeOption.User.ToString()) is null)
            await _roleManager.CreateAsync(new ApplicationRole() { Name = UserTypeOption.User.ToString() });

          await _userManager.AddToRoleAsync(user, UserTypeOption.User.ToString());
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        HttpContext.Session.SetString("UnconfirmedEmail", user.Email);

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Account",
            new { userId = user.Id, token },
            Request.Scheme
        );

        string html = $"<h1>Welcome {user.PersonName}</h1><p>Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a></p>";


        await _emailSender.SendEmailAsync(user.Email, "Confirm Your Email", html);

        return RedirectToAction(nameof(EmailVerificationSent));
      }

      foreach (var error in result.Errors)
      {
        ModelState.AddModelError("Register", error.Description);
      }
      return View(registerDTO);
    }



    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmation(string email)
    {
      if (string.IsNullOrEmpty(email)) return RedirectToAction(nameof(Login));

      var user = await _userManager.FindByEmailAsync(email);
      if (user == null) return RedirectToAction(nameof(Login));

      if (await _userManager.IsEmailConfirmedAsync(user))
        return RedirectToAction(nameof(Login));

      string cacheKey = email;
      if (_cache.TryGetValue(cacheKey, out DateTime time))
      {
        var remaning = (int)(time - DateTime.UtcNow).TotalSeconds;
        HttpContext.Session.SetString("expiry", remaning.ToString());
        return RedirectToAction(nameof(EmailVerificationSent));
      }
      else
      {
        var expiry = DateTime.UtcNow.AddMinutes(2);
        _cache.Set(cacheKey, expiry, expiry - DateTime.UtcNow);

        var remaning = (int)(expiry - DateTime.UtcNow).TotalSeconds;
        HttpContext.Session.SetString("expiry", remaning.ToString());
      }

      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      HttpContext.Session.SetString("UnconfirmedEmail", user.Email);

      var confirmationLink = Url.Action(
        "ConfirmEmail",
        "Account",
        new { userId = user.Id, Token = token }
        , Request.Scheme);
      string html = $"<h1>Hi {user.PersonName}</h1><p>Please confirm your email: <a href='{confirmationLink}'>Confirm Email</a></p>";

      await _emailSender.SendEmailAsync(user.Email, "Email Confirmation", html);

      return RedirectToAction(nameof(EmailVerificationSent));
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
      if (userId == null || token == null)
        return RedirectToAction(nameof(ConfirmEmailFailed));

      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
        return RedirectToAction(nameof(ConfirmEmailFailed));

      var result = await _userManager.ConfirmEmailAsync(user, token);
      if (result.Succeeded)
      {
        return RedirectToAction(nameof(ConfirmEmailSuccess));
      }

      return RedirectToAction(nameof(ConfirmEmailFailed));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult EmailVerificationSent()
    {
      return View();
    }
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ConfirmEmailSuccess()
    {
      return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ConfirmEmailFailed()
    {
      return View();
    }
    #endregion

    #region Forget password

    [HttpGet]
    [Authorize("NotAuthorized")]
    public IActionResult VerifyEmail()
    {
      return View();

    }

    [HttpPost]
    [Authorize("NotAuthorized")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDTO model)
    {
      if (!ModelState.IsValid)
        return View(model);

      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
      {
        ModelState.AddModelError("", "Invalid email");
        return View(model);
      }

      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      HttpContext.Session.SetString("UnconfirmedEmail", user.Email);


      var callbackUrl = Url.Action("ChangePassword", "Account", new { token, email = model.Email }, protocol: Request.Scheme);

      string htmlMessage = $"<p>Click the link to reset your password:</p><p><a href='{callbackUrl}'>Reset Password</a></p>";

      await _emailSender.SendEmailAsync(model.Email, "Reset Password", htmlMessage);


      return RedirectToAction(nameof(ForgotPasswordConfirmation));


    }
    [HttpGet]
    [AllowAnonymous]
    [Authorize("NotAuthorized")]

    public IActionResult ForgotPasswordConfirmation()
    {
      return View();
    }

    [HttpGet]
    [AllowAnonymous]
    [Authorize("NotAuthorized")]

    public async Task<IActionResult> ResendForgotPasswordEmail(string email)
    {

      if (string.IsNullOrEmpty(email))
      {
        return RedirectToAction(nameof(VerifyEmail));
      }

      var user = await _userManager.FindByEmailAsync(email);
      if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
      {
        return RedirectToActionPermanent(nameof(VerifyEmail));
      }

      string cacheKey = email + "reset";
      if (_cache.TryGetValue(cacheKey, out DateTime time))
      {
        var remaning = (int)(time - DateTime.UtcNow).TotalSeconds;
        HttpContext.Session.SetString("expiry", remaning.ToString());
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
      }
      else
      {
        var expiry = DateTime.UtcNow.AddMinutes(2);
        _cache.Set(cacheKey, expiry, expiry - DateTime.UtcNow);

        var remaning = (int)(expiry - DateTime.UtcNow).TotalSeconds;
        HttpContext.Session.SetString("expiry", remaning.ToString());
      }

      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      HttpContext.Session.SetString("UnconfirmedEmail", user.Email);


      var callbackUrl = Url.Action("ChangePassword", "Account", new { token, email = email }, protocol: Request.Scheme);

      string htmlMessage = $"<p>Click the link to reset your password:</p><p><a href='{callbackUrl}'>Reset Password</a></p>";

      await _emailSender.SendEmailAsync(email, "Reset Password", htmlMessage);


      return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [Authorize("NotAuthorized")]
    public IActionResult ChangePassword(string Email, string token)
    {
      if (string.IsNullOrEmpty(Email))
      {
        return RedirectToAction(nameof(Login));
      }

      return View(new ChangePasswordDTO { Email = Email, Token = token });
    }
    [HttpPost]
    [Authorize("NotAuthorized")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
    {
      if (!ModelState.IsValid)
      {
        ModelState.AddModelError("", "Email not found");
        return View(model);
      }

      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user is null)
      {
        ModelState.AddModelError("", "Invalid request");
        return View(model);
      }

      var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
      if (result.Succeeded)
      {
        return RedirectToAction("Login", "Account");
      }

      //foreach (var error in result.Errors)
      //{
      //  ModelState.AddModelError("", error.Description);
      //}
      ModelState.AddModelError("", "Invalid request");

      return View(model);


    }

    #endregion 

    #region Login 

    [HttpGet]
    [Authorize("NotAuthorized")]
    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    [Authorize("NotAuthorized")]
    public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
    {
      if (!ModelState.IsValid)
      {

        ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
        return View(loginDTO);
      }
      ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
      if (user == null)
      {
        ModelState.AddModelError("", "Invalid email or password");
        return View(loginDTO);
      }
      if (!await _userManager.IsEmailConfirmedAsync(user))
      {
        ViewBag.UnconfirmedEmail=user.Email;

        ModelState.AddModelError("", "Email not confirmed! Please confirm your email and try again.");
        return View(loginDTO);
      }
      
      var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, loginDTO.RememberMe, lockoutOnFailure: true);
      if (result.Succeeded)
      {
        //ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
        //if (user != null)
        //{
        //  if (await _userManager.IsInRoleAsync(user, UserTypeOption.Admin.ToString()))
        //    return RedirectToAction("Index", "Home", new { area = "Admin" });
        //}

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
          return LocalRedirect(ReturnUrl);
        }
        return RedirectToAction(nameof(PersonsController.Index), "Persons");
      }
      if (result.IsLockedOut)
      {
        ViewBag.LockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
        ViewBag.AttemptsLeft = await _userManager.GetAccessFailedCountAsync(user);

        ModelState.AddModelError("", "Your account is locked.");

        return View(loginDTO);
      }


      ModelState.AddModelError("Login", "Invalid Email or Passwprd");
      return View(loginDTO);
    }

    //login with google
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = "/")
    {
      var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = "/", string? remoteError = null)
    {
      if (remoteError != null)
      {
        ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
        return RedirectToAction(nameof(Login));
      }

      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
        return RedirectToAction(nameof(Login));

      var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

      if (result.Succeeded)
      {

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
          return LocalRedirect(returnUrl);
        }
        return RedirectToAction(nameof(PersonsController.Index), "Persons");
      }
      else
      {
        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name);

        if (email != null)
        {
          var user = await _userManager.FindByEmailAsync(email);
          if (user != null)
          {
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (addLoginResult.Succeeded)
            {
              await _signInManager.SignInAsync(user, isPersistent: false);

              if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
              {
                return LocalRedirect(returnUrl);
              }
              return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }
          }
          else
          {

            user = new ApplicationUser
            {
              UserName = email,
              Email = email,
              PersonName = name
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
              var addLoginResult = await _userManager.AddLoginAsync(user, info);
              if (addLoginResult.Succeeded)
              {
                await _signInManager.SignInAsync(user, isPersistent: false);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                  return LocalRedirect(returnUrl);
                }
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
              }
            }
          }
        }

        return RedirectToAction(nameof(Login));
      }
    }

    #endregion

    #region Logout
    [Authorize]
    public async Task<IActionResult> Logout()
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction(nameof(PersonsController.Index), "Persons");

    }
    #endregion


    [AllowAnonymous]
    public async Task<IActionResult> IsEmailAlreadyRegisteres(string email)
    {
      ApplicationUser user = await _userManager.FindByEmailAsync(email);
      return user != null ? Json(false) : Json(true);
    }
  }
}
