using Bookstore.Api.DTO.Books;

namespace Bookstore.Api.Services.Interfaces
{
    public interface IBookService
    {
        Task<IReadOnlyList<BookResponse>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<BookResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<BookResponse> CreateAsync(BookRequest request, CancellationToken cancellationToken = default);

        Task<BookResponse?> UpdateAsync(int id, BookRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
