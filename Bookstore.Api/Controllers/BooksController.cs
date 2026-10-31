using Bookstore.Api.DTO.Books;
using Bookstore.Api.DTO.Common;
using Bookstore.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [Authorize(Policy = "BookstoreRead")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await bookService.GetAllAsync(cancellationToken);
        return Ok(books);
    }

    [Authorize(Policy = "BookstoreRead")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await bookService.GetByIdAsync(id, cancellationToken);

        return book is null ? NotFound() : Ok(book);
    }
    
    [Authorize(Policy = "BookstoreWrite")]
    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(BookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await bookService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = book.BookId },
                book);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "BookstoreWrite")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookResponse>> Update(int id, BookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await bookService.UpdateAsync(id, request, cancellationToken);

            return book is null ? NotFound() : Ok(book);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "BookstoreWrite")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await bookService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [Authorize(Policy = "BookstoreRead")]
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<BookResponse>>> Search([FromQuery] BookSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await bookService.SearchAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
