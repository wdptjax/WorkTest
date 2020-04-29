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
    public class DetailsModel : PageModel
    {
        private readonly TestNetCoreWeb.Data.TestNetCoreWebContext _context;

        public DetailsModel(TestNetCoreWeb.Data.TestNetCoreWebContext context)
        {
            _context = context;
        }

        public Network Network { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Network = await _context.Network.FirstOrDefaultAsync(m => m.ID == id);

            if (Network == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
