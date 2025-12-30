using ItoCase.Core.DTOs;

namespace ItoCase.Core.Interfaces
{
    public interface IChartStrategy
    {
        string StrategyName { get; } // Örn: "CategorySales"
        Task<List<ChartDto>> GenerateDataAsync();
    }
}