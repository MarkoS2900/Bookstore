using Bookstore.Api.DTO.Books;
using Bookstore.Api.Mappings;
using Bookstore.Api.Repositories.Interfaces;
using Bookstore.Api.Services.Interfaces;

namespace Bookstore.Api.Services
{
    public class BookService(IBookRepository bookRepository) : IBookService
    {
        public async Task<IReadOnlyList<BookResponse>> GetAllAsync()
        {
            var books = await bookRepository.GetAllAsync();
            return books.Select(b => b.ToResponse()).ToList();
        }

        public async Task<BookResponse?> GetByIdAsync(int id)
        {
            var book = await bookRepository.GetByIdAsync(id);
            return book?.ToResponse();
        }

        public async Task<BookResponse> CreateAsync(BookRequest request)
        {
            var book = request.ToEntity();
            var createdBook = await bookRepository.CreateAsync(book);
            return createdBook.ToResponse();
        }

        public async Task<BookResponse> UpdateAsync(BookRequest request)
        {
            var book = request.ToEntity();
            var updatedBook = await bookRepository.UpdateAsync(book);
            return updatedBook.ToResponse();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await bookRepository.DeleteAsync(id);
        }
    }
}
