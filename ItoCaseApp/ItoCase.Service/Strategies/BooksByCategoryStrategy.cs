using ItoCase.Core.DTOs;
using ItoCase.Core.Entities;
using ItoCase.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItoCase.Service.Strategies
{
    public class BooksByCategoryStrategy : IChartStrategy
    {
        private readonly IGenericRepository<Book> _repository;

        public BooksByCategoryStrategy(IGenericRepository<Book> repository)
        {
            _repository = repository;
        }

        public string StrategyName => "BooksByCategory";

        public async Task<List<ChartDto>> GenerateDataAsync()
        {
            return await _repository.Where(x => true)
                .GroupBy(x => x.AnaKategori)
                .Select(g => new ChartDto
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();
        }
    }
}