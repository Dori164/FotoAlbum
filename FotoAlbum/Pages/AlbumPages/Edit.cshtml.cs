using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FotoAlbum.Models;
using FotoAlbum.Data;

namespace FotoAlbum.Pages.AlbumPages;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
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
        Album = album;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var albumToUpdate = await _context.Album.FindAsync(Album.Id);
        if (albumToUpdate is null)
        {
            return NotFound();
        }

        albumToUpdate.Cim = Album.Cim;
        albumToUpdate.Leiras = Album.Leiras ?? string.Empty;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AlbumExists(Album.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Details", new { id = Album.Id });
    }

    private bool AlbumExists(int id)
    {
        return _context.Album.Any(e => e.Id == id);
    }
}
