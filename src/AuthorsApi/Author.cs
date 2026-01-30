using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace AuthorsAPI
{
    public class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; } = new();
    }
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        // Navigation
        public Guid AuthorId { get; set; }
        [JsonIgnore]
        public Author Author { get; set; }
    }
    public record AuthorDTO(string Name);
    public record BookDTO(string Title/*, Guid AuthorId*/);
    public class AuthorDbContext : DbContext
    {
        public AuthorDbContext(DbContextOptions builder) : base(builder) { }

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
    }
}
