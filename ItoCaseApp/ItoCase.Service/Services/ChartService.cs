using ItoCase.Core.DTOs;
using ItoCase.Core.Interfaces;

namespace ItoCase.Service.Services
{
    public class ChartService
    {
        private readonly IEnumerable<IChartStrategy> _strategies;

        // Dependency Injection sayesinde tanımlı tüm stratejileri otomatik toplar
        public ChartService(IEnumerable<IChartStrategy> strategies)
        {
            _strategies = strategies;
        }

        public async Task<List<ChartDto>> GetDataByStrategyAsync(string strategyName)
        {
            var strategy = _strategies.FirstOrDefault(s => s.StrategyName == strategyName);

            if (strategy == null)
                throw new Exception("Böyle bir grafik türü bulunamadı.");

            return await strategy.GenerateDataAsync();
        }
    }
}