using Microsoft.AspNetCore.Identity;
using System;

namespace ContactsManager.Core.Domain.IdentityEnitities
{
  public class ApplicationUser:IdentityUser<Guid>
  {
    public string? PersonName { get; set; }
    public string? LastEmailConfirmationToken { get; set; }

  }
}
