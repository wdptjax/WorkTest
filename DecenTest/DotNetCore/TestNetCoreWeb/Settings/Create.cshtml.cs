using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TestNetCoreWeb.Data;
using TestNetCoreWeb.Settings;

namespace TestNetCoreWeb
{
    public class CreateModel : PageModel
    {
        private readonly TestNetCoreWeb.Data.TestNetCoreWebContext _context;

        public CreateModel(TestNetCoreWeb.Data.TestNetCoreWebContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Network Network { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Network.Add(Network);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
