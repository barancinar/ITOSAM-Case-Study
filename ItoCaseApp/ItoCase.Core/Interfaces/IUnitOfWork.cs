namespace ItoCase.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Hangi Repository'leri yönetiyorsa buraya eklenir
        IGenericRepository<ItoCase.Core.Entities.Book> Books { get; }

        // Değişiklikleri veritabanına ileten tek metot
        Task<int> CommitAsync();
    }
}