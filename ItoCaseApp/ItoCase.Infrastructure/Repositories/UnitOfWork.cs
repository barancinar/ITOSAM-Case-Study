using ItoCase.Core.Entities;
using ItoCase.Core.Interfaces;
using ItoCase.Infrastructure.Persistence.Context;

namespace ItoCase.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ItoCaseDbContext _context;
        private IGenericRepository<Book>? _bookRepository;

        public UnitOfWork(ItoCaseDbContext context)
        {
            _context = context;
        }

        // Eğer _bookRepository daha önce oluşturulmadıysa oluştur, yoksa var olanı ver (Singleton mantığı)
        public IGenericRepository<Book> Books =>
            _bookRepository ??= new GenericRepository<Book>(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}