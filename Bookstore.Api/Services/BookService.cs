using Bookstore.Api.DTO.Books;
using Bookstore.Api.DTO.Common;
using Bookstore.Api.Exceptions;
using System.ComponentModel.DataAnnotations;
using Bookstore.Api.Mappings;
using Bookstore.Api.Repositories.Interfaces;
using Bookstore.Api.Services.Interfaces;

namespace Bookstore.Api.Services
{
    public class BookService(IBookRepository bookRepository, IAuthorRepository authorRepository) : IBookService
    {
        public async Task<IReadOnlyList<BookResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var books = await bookRepository.GetAllAsync(cancellationToken);
            return books.Select(b => b.ToResponse()).ToList();
        }

        public async Task<BookResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var book = await bookRepository.GetByIdAsync(id, cancellationToken);
            return book?.ToResponse();
        }

        public async Task<BookResponse> CreateAsync(BookRequest request, CancellationToken cancellationToken = default)
        {
            await EnsureAuthorExistsAsync(request.AuthorId, cancellationToken);

            var book = request.ToEntity();
            var createdBook = await bookRepository.CreateAsync(book, cancellationToken);
            return createdBook.ToResponse();
        }

        public async Task<BookResponse?> UpdateAsync(int id, BookRequest request, CancellationToken cancellationToken = default)
        {
            var existingBook = await bookRepository.GetByIdAsync(id, cancellationToken);

            if (existingBook is null)
                return null;


            await EnsureAuthorExistsAsync(request.AuthorId, cancellationToken);

            existingBook.AuthorId = request.AuthorId;
            existingBook.Title = request.Title;
            existingBook.Subtitle = request.Subtitle;

            await bookRepository.UpdateAsync(existingBook, cancellationToken);

            var result = await bookRepository.GetByIdAsync(id, cancellationToken);

            return result?.ToResponse();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await bookRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<PagedResponse<BookResponse>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Page < 1)
                throw new ValidationException("Page must be greater than 0.");

            if (request.PageSize < 1 || request.PageSize > 100)
                throw new ValidationException("PageSize must be between 1 and 100.");

            var result = await bookRepository.SearchAsync(request, cancellationToken);

            return new PagedResponse<BookResponse>
            {
                Items = result.Items.Select(b => b.ToResponse()).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }

        private async Task EnsureAuthorExistsAsync(int authorId, CancellationToken cancellationToken)
        {
            if (authorId < 1)
                throw new ValidationException("AuthorId must be greater than 0.");

            if (await authorRepository.GetByIdAsync(authorId, cancellationToken) is null)
                throw new AuthorNotFoundException(authorId);
        }
    }
}
