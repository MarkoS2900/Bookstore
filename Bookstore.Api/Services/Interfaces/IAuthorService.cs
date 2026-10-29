using Bookstore.Api.DTO.Authors;

namespace Bookstore.Api.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<IReadOnlyList<AuthorResponse>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<AuthorResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken cancellationToken = default);
    }
}
