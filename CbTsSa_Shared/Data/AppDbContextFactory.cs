using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CbTsSa_Shared.DBModels;

namespace CbTsSa_Shared.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=ShakeAppDb;Username=postgres;Password=yourpassword",
                b => b.MigrationsAssembly("CbTsSa_Shared"));
            
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}