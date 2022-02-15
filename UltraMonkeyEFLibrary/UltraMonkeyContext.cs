using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMonkeyEFLibrary
{
    public class UltraMonkeyContext : DbContext
    {
        public DbSet<WeatherData> WeatherDatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            optionsBuilder
                //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
                //.EnableSensitiveDataLogging()
                .UseSqlServer(config["ConnectionStrings:DefaultConnection"]);
        }
    }
}
