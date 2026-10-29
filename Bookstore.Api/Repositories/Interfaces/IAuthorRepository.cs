using Bookstore.Api.Models;

namespace Bookstore.Api.Repositories.Interfaces
{
    public interface IAuthorRepository
    {
        Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Author> CreateAsync(Author author, CancellationToken cancellationToken = default);
    }
}
