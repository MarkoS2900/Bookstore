using Bookstore.Api.Data;
using Bookstore.Api.Models;
using Bookstore.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Repositories
{
    public class AuthorRepository(BookstoreDbContext context) : IAuthorRepository
    {
        public async Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.Authors
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Authors
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuthorId == id, cancellationToken);
        }
        public async Task<Author> CreateAsync(Author author, CancellationToken cancellationToken = default)
        {
            context.Authors.Add(author);
            await context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(author.AuthorId, cancellationToken) ?? author;
        }
    }
}
