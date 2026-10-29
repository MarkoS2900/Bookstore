using Bookstore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Data
{
    public sealed class BookstoreDbContext(DbContextOptions<BookstoreDbContext> options) : DbContext(options)
    {
        public DbSet<Book> Books => Set<Book>();

        public DbSet<Author> Authors => Set<Author>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(a => a.AuthorId);
                entity.Property(a => a.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.BookId);
                entity.Property(b => b.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasOne(b => b.Author)
                    .WithMany()
                    .HasForeignKey(b => b.AuthorId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
