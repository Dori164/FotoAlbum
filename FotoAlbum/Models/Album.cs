using System.ComponentModel.DataAnnotations;

namespace FotoAlbum.Models
{
    public class Album
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "A cím megadása kötelező.")]
        [MaxLength(80, ErrorMessage = "A cím maximum 80 karakter hosszú lehet.")]
        public string Cim { get; set; }
        public string Leiras { get; set; }
        public DateTime Letrehozva { get; set; }

        public List<Kep> Kep { get; set; } = new List<Kep>();
    }
}
