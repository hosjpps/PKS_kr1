using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Data;

public class LibraryContext : DbContext
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=library.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== Author =====
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Country)
                .HasMaxLength(100);

            entity.HasMany(a => a.Books)
                .WithOne(b => b.Author)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== Genre =====
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(g => g.Description)
                .HasMaxLength(500);

            entity.HasMany(g => g.Books)
                .WithOne(b => b.Genre)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== Book =====
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(b => b.ISBN)
                .HasMaxLength(20);

            entity.Property(b => b.PublishYear)
                .IsRequired();

            entity.Property(b => b.QuantityInStock)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(b => b.AuthorId)
                .IsRequired();

            entity.Property(b => b.GenreId)
                .IsRequired();
        });
    }

    /// <summary>
    /// Seeds the database with initial sample data if empty.
    /// </summary>
    public static void SeedData(LibraryContext context)
    {
        if (context.Authors.Any())
            return;

        // Authors
        var pushkin = new Author
        {
            FirstName = "Александр",
            LastName = "Пушкин",
            BirthDate = new DateTime(1799, 6, 6),
            Country = "Россия"
        };
        var tolstoy = new Author
        {
            FirstName = "Лев",
            LastName = "Толстой",
            BirthDate = new DateTime(1828, 9, 9),
            Country = "Россия"
        };
        var dostoevsky = new Author
        {
            FirstName = "Фёдор",
            LastName = "Достоевский",
            BirthDate = new DateTime(1821, 11, 11),
            Country = "Россия"
        };
        var chekhov = new Author
        {
            FirstName = "Антон",
            LastName = "Чехов",
            BirthDate = new DateTime(1860, 1, 29),
            Country = "Россия"
        };

        context.Authors.AddRange(pushkin, tolstoy, dostoevsky, chekhov);
        context.SaveChanges();

        // Genres
        var roman = new Genre { Name = "Роман", Description = "Прозаический жанр, большое по объёму произведение" };
        var poetry = new Genre { Name = "Поэзия", Description = "Стихотворные произведения" };
        var story = new Genre { Name = "Повесть", Description = "Прозаическое произведение среднего объёма" };
        var drama = new Genre { Name = "Драма", Description = "Произведение для постановки на сцене" };

        context.Genres.AddRange(roman, poetry, story, drama);
        context.SaveChanges();

        // Books
        context.Books.AddRange(
            new Book
            {
                Title = "Евгений Онегин",
                AuthorId = pushkin.Id,
                GenreId = roman.Id,
                PublishYear = 1833,
                ISBN = "978-5-17-090000-1",
                QuantityInStock = 5
            },
            new Book
            {
                Title = "Капитанская дочка",
                AuthorId = pushkin.Id,
                GenreId = story.Id,
                PublishYear = 1836,
                ISBN = "978-5-17-090000-2",
                QuantityInStock = 3
            },
            new Book
            {
                Title = "Война и мир",
                AuthorId = tolstoy.Id,
                GenreId = roman.Id,
                PublishYear = 1869,
                ISBN = "978-5-17-090000-3",
                QuantityInStock = 7
            },
            new Book
            {
                Title = "Анна Каренина",
                AuthorId = tolstoy.Id,
                GenreId = roman.Id,
                PublishYear = 1877,
                ISBN = "978-5-17-090000-4",
                QuantityInStock = 4
            },
            new Book
            {
                Title = "Преступление и наказание",
                AuthorId = dostoevsky.Id,
                GenreId = roman.Id,
                PublishYear = 1866,
                ISBN = "978-5-17-090000-5",
                QuantityInStock = 6
            },
            new Book
            {
                Title = "Братья Карамазовы",
                AuthorId = dostoevsky.Id,
                GenreId = roman.Id,
                PublishYear = 1880,
                ISBN = "978-5-17-090000-6",
                QuantityInStock = 2
            },
            new Book
            {
                Title = "Вишнёвый сад",
                AuthorId = chekhov.Id,
                GenreId = drama.Id,
                PublishYear = 1904,
                ISBN = "978-5-17-090000-7",
                QuantityInStock = 8
            },
            new Book
            {
                Title = "Три сестры",
                AuthorId = chekhov.Id,
                GenreId = drama.Id,
                PublishYear = 1901,
                ISBN = "978-5-17-090000-8",
                QuantityInStock = 1
            }
        );
        context.SaveChanges();
    }
}
