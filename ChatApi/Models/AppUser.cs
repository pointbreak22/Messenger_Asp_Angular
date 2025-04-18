using Microsoft.AspNetCore.Identity;

namespace ChatApi.Models;

public class AppUser:IdentityUser
{
    public string? FullName { get; set; }
    public string? ProfileImage { get; set; }
}   