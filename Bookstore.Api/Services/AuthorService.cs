using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Mappings;
using Bookstore.Api.Repositories.Interfaces;
using Bookstore.Api.Services.Interfaces;

namespace Bookstore.Api.Services
{
    public class AuthorService(IAuthorRepository authorRepository) : IAuthorService
    {
        public async Task<IReadOnlyList<AuthorResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var authors = await authorRepository.GetAllAsync(cancellationToken);
            return authors.Select(a => a.ToResponse()).ToList();
        }

        public async Task<AuthorResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var author = await authorRepository.GetByIdAsync(id, cancellationToken);
            return author?.ToResponse();
        }
        public async Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken cancellationToken = default)
        {
            var author = request.ToEntity();
            var createdAuthor = await authorRepository.CreateAsync(author, cancellationToken);
            return createdAuthor.ToResponse();
        }
    }
}
