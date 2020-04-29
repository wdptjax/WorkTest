using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestNetCoreWeb.Data;
using TestNetCoreWeb.Settings;

namespace TestNetCoreWeb
{
    public class EditModel : PageModel
    {
        private readonly TestNetCoreWeb.Data.TestNetCoreWebContext _context;

        public EditModel(TestNetCoreWeb.Data.TestNetCoreWebContext context)
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

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Network).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NetworkExists(Network.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool NetworkExists(int id)
        {
            return _context.Network.Any(e => e.ID == id);
        }
    }
}
