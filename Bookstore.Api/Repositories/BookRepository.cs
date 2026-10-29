using Bookstore.Api.Data;
using Bookstore.Api.Models;
using Bookstore.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Repositories
{
    public class BookRepository(BookstoreDbContext context) : IBookRepository
    {
        public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .OrderBy(b => b.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BookId == id, cancellationToken);
        }
        public async Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default)
        {
            context.Books.Add(book);
            await context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(book.BookId, cancellationToken) ?? book;
        }

        public async Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default)
        {
            var entity = await context.Books.FindAsync(book.BookId, cancellationToken) ?? throw new InvalidOperationException($"Book {book.Title} not found");

            context.Entry(entity).CurrentValues.SetValues(book);
            await context.SaveChangesAsync(cancellationToken);
            return book;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var book = await context.Books.FindAsync(id, cancellationToken);
            if (book == null)
                return false;

            context.Books.Remove(book);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
