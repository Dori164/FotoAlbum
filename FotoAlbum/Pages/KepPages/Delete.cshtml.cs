using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FotoAlbum.Models;
using FotoAlbum.Data;

namespace FotoAlbum.Pages.KepPages;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;

    public DeleteModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Kep Kep { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var kep = await _context.Kep.FirstOrDefaultAsync(m => m.Id == id);
        if (kep is null)
        {
            return NotFound();
        }
        else
        {
            Kep = kep;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var kep = await _context.Kep.FindAsync(id);
        if (kep != null)
        {
            Kep = kep;
            _context.Kep.Remove(Kep);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
