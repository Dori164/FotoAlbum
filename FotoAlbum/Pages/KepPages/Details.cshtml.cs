using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FotoAlbum.Models;
using FotoAlbum.Data;

namespace FotoAlbum.Pages.KepPages;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;
    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

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
}
