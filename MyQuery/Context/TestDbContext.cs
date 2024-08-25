using Microsoft.EntityFrameworkCore;
using Synergy_Test.Models;

namespace Synergy_Test.Context
{
    public class TestDbContext:DbContext
    {

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet <LoginModel> Login { get; set; }
    }
}
