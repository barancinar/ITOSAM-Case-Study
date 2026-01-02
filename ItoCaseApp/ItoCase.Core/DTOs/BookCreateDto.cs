using System.ComponentModel.DataAnnotations;

namespace ItoCase.Core.DTOs
{
    public class BookCreateDto
    {
        [Required(ErrorMessage = "Kitap adı zorunludur.")]
        public string KitapAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yazar adı zorunludur.")]
        public string Yazar { get; set; } = string.Empty;

        public string? Yayinevi { get; set; }
        public string? AnaKategori { get; set; }
        public string? Turu { get; set; } // Roman, Hikaye vb.
        
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan küçük olamaz.")]
        public decimal Fiyat { get; set; }

        public int SatisRakamlari { get; set; }
        public string? ISBN { get; set; }
        public string? SayfaSayisi { get; set; }
        public string? BasimYili { get; set; }
        public string? KagitTipi { get; set; }
        public string? KapakTipi { get; set; }
    }
}
