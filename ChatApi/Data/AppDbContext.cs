using ChatApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Data;

public class AppDbContext:IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
        
        
    }
    public DbSet<Message> Messages { get; set; }

}