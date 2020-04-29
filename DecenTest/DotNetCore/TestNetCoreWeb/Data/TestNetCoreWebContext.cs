using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestNetCoreWeb.Settings;

namespace TestNetCoreWeb.Data
{
    public class TestNetCoreWebContext : DbContext
    {
        public TestNetCoreWebContext (DbContextOptions<TestNetCoreWebContext> options)
            : base(options)
        {
        }

        public DbSet<TestNetCoreWeb.Settings.Network> Network { get; set; }
    }
}
