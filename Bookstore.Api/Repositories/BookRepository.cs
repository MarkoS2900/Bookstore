using Bookstore.Api.Data;
using Bookstore.Api.DTO.Books;
using Bookstore.Api.DTO.Common;
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

        public async Task<PagedResponse<Book>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(b =>
                    b.Title.Contains(request.Title));
            }

            if (!string.IsNullOrWhiteSpace(request.Author))
            {
                query = query.Where(b =>
                    b.Author.Name.Contains(request.Author));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var books = await query
                .OrderBy(b => b.Title)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<Book>
            {
                Items = books,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
