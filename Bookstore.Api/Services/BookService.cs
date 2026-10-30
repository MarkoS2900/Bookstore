using Bookstore.Api.DTO.Books;
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
            var book = request.ToEntity();
            var createdBook = await bookRepository.CreateAsync(book, cancellationToken);
            return createdBook.ToResponse();
        }

        public async Task<BookResponse?> UpdateAsync(int id, BookRequest request, CancellationToken cancellationToken = default)
        {
            var existingBook = await bookRepository.GetByIdAsync(id, cancellationToken);

            if (existingBook is null)
                return null;


            var author = await authorRepository.GetByIdAsync(request.AuthorId, cancellationToken);

            if (author is null)
                throw new KeyNotFoundException(
                    $"Author with ID {request.AuthorId} was not found.");

            existingBook.AuthorId = request.AuthorId;
            existingBook.Title = request.Title;
            existingBook.Subtitle = request.Subtitle;

            var updatedBook = await bookRepository.UpdateAsync(
                existingBook, cancellationToken);

            var result = await bookRepository.GetByIdAsync(id, cancellationToken);

            return result?.ToResponse();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await bookRepository.DeleteAsync(id, cancellationToken);
        }
    }
}
