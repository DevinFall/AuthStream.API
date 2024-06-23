using AuthStream.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthStream.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users => Set<User>();
}