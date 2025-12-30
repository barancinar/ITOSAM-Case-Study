using ItoCase.Core.DTOs;
using ItoCase.Core.Entities;
using ItoCase.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItoCase.Service.Strategies
{
    public class BestSellersStrategy : IChartStrategy
    {
        private readonly IGenericRepository<Book> _repository;

        public BestSellersStrategy(IGenericRepository<Book> repository)
        {
            _repository = repository;
        }

        // Bu isim HTML'deki <option value="..."> ile AYNI olmalı
        public string StrategyName => "BestSellers";

        public async Task<List<ChartDto>> GenerateDataAsync()
        {
            // Satış rakamına göre çoktan aza sırala ve ilk 10'u al
            var data = await _repository.Where(x => true)
                .OrderByDescending(x => x.SatisRakamlari)
                .Take(10)
                .Select(x => new ChartDto
                {
                    // Grafik etiketi: Kitap Adı (Kısa olsun diye ilk 20 karakter)
                    Label = !string.IsNullOrEmpty(x.KitapAdi)
                        ? (x.KitapAdi.Length > 20
                            ? x.KitapAdi.Substring(0, 20) + "..."
                            : x.KitapAdi)
                        : "Bilinmeyen Kitap",

                    // Grafik değeri: Satış Adedi
                    Value = x.SatisRakamlari
                })
                .ToListAsync();

            return data;
        }
    }
}