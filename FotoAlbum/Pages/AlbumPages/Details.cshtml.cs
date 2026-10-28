using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FotoAlbum.Models;
using FotoAlbum.Data;

namespace FotoAlbum.Pages.AlbumPages;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;
    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public Album Album { get; set; } = default!;

    [BindProperty]
    public string NewKepCim { get; set; } = string.Empty;

    [BindProperty]
    public string NewKepUrl { get; set; } = string.Empty;

    [BindProperty]
    public string NewKepLeiras { get; set; } = string.Empty;

    [BindProperty]
    public int EditKepId { get; set; }

    [BindProperty]
    public string EditKepCim { get; set; } = string.Empty;

    [BindProperty]
    public string EditKepUrl { get; set; } = string.Empty;

    [BindProperty]
    public string EditKepLeiras { get; set; } = string.Empty;

    [BindProperty]
    public int MoveKepId { get; set; }

    [BindProperty]
    public int TargetAlbumId { get; set; }

    public IList<Album> Albums { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var album = await _context.Album
            .Include(item => item.Kep)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (album is null)
        {
            return NotFound();
        }
        else
        {
            Album = album;
        }

        await LoadAlbumsAsync(album.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAddKepAsync(int id)
    {
        var album = await LoadAlbumAsync(id);
        if (album is null)
        {
            return NotFound();
        }

        if (await _context.Kep.CountAsync(kep => kep.AlbumId == id) >= 20)
        {
            ModelState.AddModelError(string.Empty, "Egy albumban legfeljebb 20 kép lehet.");
        }

        if (string.IsNullOrWhiteSpace(NewKepCim))
        {
            ModelState.AddModelError(nameof(NewKepCim), "A cím megadása kötelező.");
        }

        if (!IsHttpUrl(NewKepUrl))
        {
            ModelState.AddModelError(nameof(NewKepUrl), "Csak érvényes http vagy https URL adható meg.");
        }

        if (!ModelState.IsValid)
        {
            return await PageWithAlbumAsync(album);
        }

        _context.Kep.Add(new Kep
        {
            AlbumId = id,
            Cim = NewKepCim,
            KepUrl = NewKepUrl,
            Leiras = NewKepLeiras ?? string.Empty,
            FeltoltesDatuma = DateTime.Now
        });
        await _context.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostEditKepAsync(int id)
    {
        var kep = await _context.Kep.FirstOrDefaultAsync(item => item.Id == EditKepId && item.AlbumId == id);
        if (kep is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(EditKepCim))
        {
            ModelState.AddModelError(nameof(EditKepCim), "A cím megadása kötelező.");
        }

        if (!IsHttpUrl(EditKepUrl))
        {
            ModelState.AddModelError(nameof(EditKepUrl), "Csak érvényes http vagy https URL adható meg.");
        }

        if (!ModelState.IsValid)
        {
            var album = await LoadAlbumAsync(id);
            return album is null ? NotFound() : await PageWithAlbumAsync(album);
        }

        kep.Cim = EditKepCim;
        kep.KepUrl = EditKepUrl;
        kep.Leiras = EditKepLeiras;
        await _context.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteKepAsync(int id, int kepId)
    {
        var kep = await _context.Kep.FirstOrDefaultAsync(item => item.Id == kepId && item.AlbumId == id);
        if (kep is null)
        {
            return NotFound();
        }

        _context.Kep.Remove(kep);
        await _context.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostMoveKepAsync(int id)
    {
        var kep = await _context.Kep.FirstOrDefaultAsync(item => item.Id == MoveKepId && item.AlbumId == id);
        if (kep is null)
        {
            return NotFound();
        }

        if (TargetAlbumId == id)
        {
            ModelState.AddModelError(nameof(TargetAlbumId), "Az aktuális album nem lehet a célalbum.");
        }

        var targetAlbum = await _context.Album.FindAsync(TargetAlbumId);
        if (targetAlbum is null)
        {
            ModelState.AddModelError(nameof(TargetAlbumId), "A célalbum nem található.");
        }
        else if (await _context.Kep.CountAsync(item => item.AlbumId == TargetAlbumId) >= 20)
        {
            ModelState.AddModelError(nameof(TargetAlbumId), "A célalbumban már 20 kép van.");
        }

        if (!ModelState.IsValid)
        {
            var album = await LoadAlbumAsync(id);
            return album is null ? NotFound() : await PageWithAlbumAsync(album);
        }

        kep.AlbumId = TargetAlbumId;
        await _context.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    private async Task<Album?> LoadAlbumAsync(int id)
    {
        return await _context.Album
            .Include(item => item.Kep)
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    private async Task LoadAlbumsAsync(int currentAlbumId)
    {
        Albums = await _context.Album
            .Where(item => item.Id != currentAlbumId)
            .OrderBy(item => item.Cim)
            .ToListAsync();
    }

    private async Task<IActionResult> PageWithAlbumAsync(Album album)
    {
        Album = album;
        await LoadAlbumsAsync(album.Id);
        return Page();
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
