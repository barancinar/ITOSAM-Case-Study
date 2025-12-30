using ItoCase.Core.DTOs;

namespace ItoCase.Core.Interfaces
{
    public interface IBookService
    {
        Task ImportExcelToDatabaseAsync(string filePath);
        Task<List<BookDto>> GetAllBooksAsync();
        Task<DataTableResponseDto<BookDto>> GetBooksForDataTableAsync(DataTableRequestDto request);
    }
}