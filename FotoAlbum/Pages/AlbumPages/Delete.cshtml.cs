using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FotoAlbum.Models;
using FotoAlbum.Data;

namespace FotoAlbum.Pages.AlbumPages;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;

    public DeleteModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Album Album { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var album = await _context.Album.FirstOrDefaultAsync(m => m.Id == id);
        if (album is null)
        {
            return NotFound();
        }
        else
        {
            Album = album;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var album = await _context.Album.FindAsync(id);
        if (album != null)
        {
            Album = album;
            _context.Album.Remove(Album);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
