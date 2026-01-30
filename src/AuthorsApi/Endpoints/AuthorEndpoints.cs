using AuthorsAPI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorsAPI.Endpoints;

public static class AuthorEndpoints
{
    public static async Task<Results<Created<Author>, ProblemHttpResult>> CreateAuthor(AuthorDTO dto, AuthorDbContext context)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid author name",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Author name cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        var isDuplicate = await context.Authors.AnyAsync(
            author => author.Name.Equals(dto.Name, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            var pd = new ProblemDetails
            {
                Title = "Duplicate author",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author name '{dto.Name}' already exists"
            };

            return TypedResults.Problem(pd);
        }

        var author = new Author
        {
            Name = dto.Name,
        };

        await context.AddAsync(author);
        await context.SaveChangesAsync();

        return TypedResults.Created($"/authors/{author.Id}", author);
    }

    public static async Task<Results<Ok<List<Author>>, ProblemHttpResult>> GetAuthors(AuthorDbContext context)
    {
        var authors = await context.Authors.ToListAsync();
        return TypedResults.Ok(authors);
    }

    public static async Task<Results<Ok<Author>, ProblemHttpResult>> GetAuthor(
        [FromRoute]Guid id,
        AuthorDbContext context,
        [FromQuery] bool includeBooks = false)
    {
        if (id == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        IQueryable<Author> author = context.Authors
                            .Where(a => a.Id == id);

        if (!await author.AnyAsync()) // alternativt: author.Count() == 0
        {
            var pd = new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        if (includeBooks)
        {
            author = author.Include(a => a.Books);     // Eager loading
        }

        return TypedResults.Ok(await author.FirstOrDefaultAsync());
    }

    public static async Task<Results<Created<Book>, ProblemHttpResult>> CreateBook([FromRoute] Guid authorId, BookDTO dto, AuthorDbContext context)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid Book Title",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Book title cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        var author = await context.Authors.FindAsync(new object[] { authorId });

        if (author is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{authorId}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        var isDuplicate = await context.Books.AnyAsync(
            book => book.AuthorId == authorId &&
                    book.Title.Equals(dto.Title, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            var pd = new ProblemDetails
            {
                Title = "Book Title Already Exists",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author with ID '{author.Name}' has already written a book with the title '{dto.Title}'"
            };

            return TypedResults.Problem(pd);
        }

        var book = new Book
        {
            Title = dto.Title,
            AuthorId = authorId,
            Author = author
        };

        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        return TypedResults.Created($"/books/{book.Id}", book);
    }
}
