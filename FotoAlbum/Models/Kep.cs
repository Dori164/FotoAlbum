using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FotoAlbum.Models
{
    public class Kep
    {
        public int Id { get; set; }
        [ForeignKey("Album")]
        public int AlbumId { get; set; }
        [Required(ErrorMessage = "A cím megadása kötelező.")]
        [MaxLength(80, ErrorMessage = "A cím maximum 80 karakter hosszú lehet.")]
        public string? Cim { get; set; }
        [Required(ErrorMessage = "A képhez érvényes url-t kell megadni.")]
        [Url(ErrorMessage = "A megadott url nem érvényes.")]
        public string? KepUrl { get; set; }
        public string? Leiras { get; set; }
        public DateTime FeltoltesDatuma { get; set; }

        public Album? Album { get; set; }



    }
}
