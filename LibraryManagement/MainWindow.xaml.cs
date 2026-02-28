using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadFilters();
        LoadBooks();
    }

    private void LoadFilters()
    {
        using var context = new LibraryContext();

        // Author filter
        var authors = context.Authors.OrderBy(a => a.LastName).ToList();
        AuthorFilter.Items.Clear();
        AuthorFilter.Items.Add(new ComboBoxItem { Content = "Все авторы", Tag = 0 });
        foreach (var author in authors)
        {
            AuthorFilter.Items.Add(new ComboBoxItem { Content = author.FullName, Tag = author.Id });
        }
        AuthorFilter.SelectedIndex = 0;

        // Genre filter
        var genres = context.Genres.OrderBy(g => g.Name).ToList();
        GenreFilter.Items.Clear();
        GenreFilter.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = 0 });
        foreach (var genre in genres)
        {
            GenreFilter.Items.Add(new ComboBoxItem { Content = genre.Name, Tag = genre.Id });
        }
        GenreFilter.SelectedIndex = 0;
    }

    private void LoadBooks()
    {
        using var context = new LibraryContext();

        IQueryable<Book> query = context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre);

        // Search filter
        string searchText = SearchBox.Text.Trim();
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(b => b.Title.Contains(searchText));
        }

        // Author filter
        if (AuthorFilter.SelectedItem is ComboBoxItem authorItem && authorItem.Tag is int authorId && authorId > 0)
        {
            query = query.Where(b => b.AuthorId == authorId);
        }

        // Genre filter
        if (GenreFilter.SelectedItem is ComboBoxItem genreItem && genreItem.Tag is int genreId && genreId > 0)
        {
            query = query.Where(b => b.GenreId == genreId);
        }

        var books = query.OrderBy(b => b.Title).ToList();
        BooksGrid.ItemsSource = books;

        // Update status
        int totalBooks = context.Books.Count();
        int totalInStock = context.Books.Sum(b => b.QuantityInStock);
        StatusText.Text = $"Показано книг: {books.Count}";
        TotalBooksText.Text = $"Всего в библиотеке: {totalBooks} книг, {totalInStock} экземпляров";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadBooks();
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
            LoadBooks();
    }

    private void ResetFilters_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        AuthorFilter.SelectedIndex = 0;
        GenreFilter.SelectedIndex = 0;
        LoadBooks();
    }

    private void AddBook_Click(object sender, RoutedEventArgs e)
    {
        var window = new BookEditWindow();
        window.Owner = this;
        if (window.ShowDialog() == true)
        {
            LoadFilters();
            LoadBooks();
        }
    }

    private void EditBook_Click(object sender, RoutedEventArgs e)
    {
        EditSelectedBook();
    }

    private void BooksGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        EditSelectedBook();
    }

    private void EditSelectedBook()
    {
        if (BooksGrid.SelectedItem is not Book selectedBook)
        {
            MessageBox.Show("Выберите книгу для редактирования.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new BookEditWindow(selectedBook.Id);
        window.Owner = this;
        if (window.ShowDialog() == true)
        {
            LoadFilters();
            LoadBooks();
        }
    }

    private void DeleteBook_Click(object sender, RoutedEventArgs e)
    {
        if (BooksGrid.SelectedItem is not Book selectedBook)
        {
            MessageBox.Show("Выберите книгу для удаления.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Удалить книгу \"{selectedBook.Title}\"?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            using var context = new LibraryContext();
            var book = context.Books.Find(selectedBook.Id);
            if (book != null)
            {
                context.Books.Remove(book);
                context.SaveChanges();
            }
            LoadBooks();
        }
    }

    private void ManageAuthors_Click(object sender, RoutedEventArgs e)
    {
        var window = new AuthorsWindow();
        window.Owner = this;
        window.ShowDialog();
        LoadFilters();
        LoadBooks();
    }

    private void ManageGenres_Click(object sender, RoutedEventArgs e)
    {
        var window = new GenresWindow();
        window.Owner = this;
        window.ShowDialog();
        LoadFilters();
        LoadBooks();
    }
}
