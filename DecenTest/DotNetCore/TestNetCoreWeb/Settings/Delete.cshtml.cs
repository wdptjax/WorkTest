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
    public class DeleteModel : PageModel
    {
        private readonly TestNetCoreWeb.Data.TestNetCoreWebContext _context;

        public DeleteModel(TestNetCoreWeb.Data.TestNetCoreWebContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Network = await _context.Network.FindAsync(id);

            if (Network != null)
            {
                _context.Network.Remove(Network);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
