//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;
//using System.IO;

//namespace PRESAM.Infrastructure.Context
//{
//    public class PresamDbContextFactory : IDesignTimeDbContextFactory<PresamDbContext>
//    {
//        public PresamDbContext CreateDbContext(string[] args)
//        {
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PRESAM.Web"))
//                .AddJsonFile("appsettings.json", optional: false)
//                .AddJsonFile("appsettings.Development.json", optional: true)
//                .Build();

//            // Configure options
//            var optionsBuilder = new DbContextOptionsBuilder<PresamDbContext>();
//            var connectionString = configuration.GetConnectionString("DefaultConnection");

//            optionsBuilder.UseSqlServer(connectionString);

//            return new PresamDbContext(optionsBuilder.Options);
//        }
//    }
//}