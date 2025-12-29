using ItoCase.Core.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItoCase.Core.Entities
{
    public class Book : BaseEntity
    {
        public string? AnaKategori { get; set; } // Örn: Aksiyon
        public string? Turu { get; set; }        // Örn: Rus Edebiyatı
        public string? KitapAdi { get; set; }
        public string? Yazar { get; set; }
        public string? Yayinevi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fiyat { get; set; }

        public string? ISBN { get; set; }
        public string? SayfaSayisi { get; set; }
        public string? BasimYili { get; set; }
        public string? KagitTipi { get; set; }
        public string? KapakTipi { get; set; }

        public int SatisRakamlari { get; set; }
    }
}