using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TestNetCoreWeb.Data;
using TestNetCoreWeb.Settings;

namespace TestNetCoreWeb
{
    public class IndexModel : PageModel
    {
        private readonly TestNetCoreWeb.Data.TestNetCoreWebContext _context;

        public IndexModel(TestNetCoreWeb.Data.TestNetCoreWebContext context)
        {
            _context = context;
        }

        public IList<Network> Network { get;set; }

        public async Task OnGetAsync()
        {
            Network = await _context.Network.ToListAsync();
        }
    }
}
