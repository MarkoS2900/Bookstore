using Bookstore.Api.DTO.Books;
using Bookstore.Api.DTO.Common;
using Bookstore.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BooksController(IBookService bookService, ILogger<BooksController> logger) : ControllerBase
{
    [Authorize(Policy = "BookstoreRead")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all books.");

        var books = await bookService.GetAllAsync(cancellationToken);

        logger.LogInformation("Successfully retrieved {BookCount} books.", books.Count);

        return Ok(books);
    }

    [Authorize(Policy = "BookstoreRead")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving book with ID {BookId}.", id);

        var book = await bookService.GetByIdAsync(id, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Book with ID {BookId} was not found.", id);
            return NotFound();
        }

        logger.LogInformation("Successfully retrieved book with ID {BookId}.", id);

        return Ok(book);
    }
    
    [Authorize(Policy = "BookstoreWrite")]
    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(BookRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating book.");

        var book = await bookService.CreateAsync(request, cancellationToken);

        logger.LogInformation("Successfully created book with ID {BookId}.", book.BookId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = book.BookId },
            book);
    }

    [Authorize(Policy = "BookstoreWrite")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookResponse>> Update(int id, BookRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating book with ID {BookId}.", id);
        var book = await bookService.UpdateAsync(id, request, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Book with ID {BookId} was not found.", id);
            return NotFound();
        }

        logger.LogInformation("Successfully updated book with ID {BookId}.", id);
        return Ok(book);
    }

    [Authorize(Policy = "BookstoreWrite")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting book with ID {BookId}.", id);
        var deleted = await bookService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            logger.LogWarning("Book with ID {BookId} was not found.", id);
            return NotFound();
        }

        logger.LogInformation("Successfully deleted book with ID {BookId}.", id);
        return NoContent();
    }

    [Authorize(Policy = "BookstoreRead")]
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<BookResponse>>> Search([FromQuery] BookSearchRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching for books with provided filters.");
        var result = await bookService.SearchAsync(request, cancellationToken);
        logger.LogInformation("Successfully found {BookCount} books.", result.Items.Count());
        return Ok(result);
    }
}
