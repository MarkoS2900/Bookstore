using Bookstore.Api.DTO.Books;

namespace Bookstore.Api.Services.Interfaces
{
    public interface IBookService
    {
        Task<IReadOnlyList<BookResponse>> GetAllAsync();

        Task<BookResponse?> GetByIdAsync(int id);

        Task<BookResponse> CreateAsync(BookRequest request);

        Task<BookResponse> UpdateAsync(BookRequest request);

        Task<bool> DeleteAsync(int id);
    }
}
