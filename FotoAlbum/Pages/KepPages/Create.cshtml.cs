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
    public string? KepUrl { get; set; }

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

        var hasFile = FeltoltottFajl is { Length: > 0 };
        if (hasFile && !IsImage(FeltoltottFajl!))
        {
            ModelState.AddModelError(nameof(FeltoltottFajl), "Csak JPG, JPEG, JFIF, PNG, GIF, WEBP vagy AVIF kép tölthető fel.");
        }
        else if (hasFile && FeltoltottFajl!.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(FeltoltottFajl), "A kép legfeljebb 5 MB méretű lehet.");
        }

        if (!hasFile && !IsHttpUrl(KepUrl))
        {
            ModelState.AddModelError(nameof(KepUrl), "Adj meg érvényes http vagy https kép URL-t, vagy tölts fel egy képfájlt.");
        }

        if (!ModelState.IsValid)
        {
            Album = album;
            AlbumId = selectedAlbumId;
            await LoadAlbumsAsync();
            return Page();
        }

        string? filePath = null;
        var imageUrl = KepUrl?.Trim() ?? string.Empty;
        if (hasFile)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            var extension = Path.GetExtension(FeltoltottFajl!.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.CreateNew);
            await FeltoltottFajl.CopyToAsync(stream);
            imageUrl = $"/uploads/{fileName}";
        }

        var kep = new Kep
        {
            AlbumId = selectedAlbumId,
            Cim = Cim!.Trim(),
            KepUrl = imageUrl,
            Leiras = Leiras ?? string.Empty,
            FeltoltesDatuma = DateTime.Now,
            Album = album
        };
        _context.Kep.Add(kep);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            if (filePath is not null)
            {
                System.IO.File.Delete(filePath);
            }
            _context.Entry(kep).State = EntityState.Detached;
            ModelState.AddModelError(string.Empty, $"A kép mentése sikertelen: {exception.InnerException?.Message ?? exception.Message}");
            Album = album;
            AlbumId = selectedAlbumId;
            await LoadAlbumsAsync();
            return Page();
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
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".jfif", ".png", ".gif", ".webp", ".avif" };
        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;

        return allowedExtensions.Contains(extension)
            && (string.IsNullOrWhiteSpace(contentType) || contentType.StartsWith("image/"));
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
