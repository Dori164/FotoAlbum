using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FotoAlbum.Models;
using FotoAlbum.Data;

namespace FotoAlbum.Pages.KepPages;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CreateModel(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public Album? Album { get; set; }

    public IList<Album> Albums { get; set; } = [];

    public Kep Kep { get; set; } = new();

    [BindProperty]
    public int AlbumId { get; set; }

    [BindProperty]
    public string? Cim { get; set; }

    [BindProperty]
    public string? Leiras { get; set; }

    [BindProperty]
    public IFormFile? FeltoltottFajl { get; set; }

    public async Task<IActionResult> OnGetAsync(int? albumId)
    {
        if (albumId is not null)
        {
            Album = await _context.Album.FindAsync(albumId.Value);
            if (Album is null)
            {
                return NotFound();
            }

            AlbumId = Album.Id;
        }
        else
        {
            await LoadAlbumsAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? albumId)
    {
        var selectedAlbumId = albumId ?? AlbumId;
        var album = await _context.Album.FindAsync(selectedAlbumId);
        if (album is null)
        {
            ModelState.AddModelError(nameof(AlbumId), "Válassz albumot.");
        }

        if (string.IsNullOrWhiteSpace(Cim))
        {
            ModelState.AddModelError(nameof(Cim), "A cím megadása kötelező.");
        }
        else if (Cim.Length > 80)
        {
            ModelState.AddModelError(nameof(Cim), "A cím maximum 80 karakter hosszú lehet.");
        }

        if (album is not null && await _context.Kep.CountAsync(kep => kep.AlbumId == selectedAlbumId) >= 20)
        {
            ModelState.AddModelError(string.Empty, "Egy albumban legfeljebb 20 kép lehet.");
        }

        if (FeltoltottFajl is null || FeltoltottFajl.Length == 0)
        {
            ModelState.AddModelError(nameof(FeltoltottFajl), "Válassz ki egy képfájlt.");
        }
        else if (FeltoltottFajl.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(FeltoltottFajl), "A kép legfeljebb 5 MB méretű lehet.");
        }
        else if (!IsImage(FeltoltottFajl))
        {
            ModelState.AddModelError(nameof(FeltoltottFajl), "Csak JPG, PNG, GIF vagy WEBP kép tölthető fel.");
        }

        if (!ModelState.IsValid)
        {
            Album = album;
            AlbumId = selectedAlbumId;
            await LoadAlbumsAsync();
            return Page();
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var extension = Path.GetExtension(FeltoltottFajl!.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await FeltoltottFajl.CopyToAsync(stream);
        }

        var kep = new Kep
        {
            AlbumId = selectedAlbumId,
            Cim = Cim!.Trim(),
            KepUrl = $"/uploads/{fileName}",
            Leiras = Leiras ?? string.Empty,
            FeltoltesDatuma = DateTime.Now
        };
        _context.Kep.Add(kep);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            System.IO.File.Delete(filePath);
            throw;
        }

        return RedirectToPage("/AlbumPages/Details", new { id = selectedAlbumId });
    }

    private async Task LoadAlbumsAsync()
    {
        Albums = await _context.Album
            .OrderBy(album => album.Cim)
            .ToListAsync();
    }

    private static bool IsImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

        return allowedExtensions.Contains(extension) && allowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
    }
}
