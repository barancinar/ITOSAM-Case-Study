using ItoCase.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ItoCase.Infrastructure.Persistence.Context
{
    public class ItoCaseDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public ItoCaseDbContext(DbContextOptions<ItoCaseDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}