using Bookstore.Api.DTO.Books;
using Bookstore.Api.DTO.Common;
using Bookstore.Api.Models;

namespace Bookstore.Api.Repositories.Interfaces
{
    public interface IBookRepository
    {
        Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default);

        Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<PagedResponse<Book>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default);
    }
}
