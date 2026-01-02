namespace ItoCase.Core.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string? AnaKategori { get; set; }
        public string? Turu { get; set; }
        public string? KitapAdi { get; set; }
        public string? Yazar { get; set; }
        public string? Yayinevi { get; set; }
        public decimal? Fiyat { get; set; }
        public string? ISBN { get; set; }
        public string? SayfaSayisi { get; set; }
        public string? BasimYili { get; set; }
        public string? KagitTipi { get; set; }
        public string? KapakTipi { get; set; }
        public int SatisRakamlari { get; set; }
    }
}