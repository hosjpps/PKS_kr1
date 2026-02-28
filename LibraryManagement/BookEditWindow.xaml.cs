using System.Windows;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement;

public partial class BookEditWindow : Window
{
    private readonly int? _bookId;

    public BookEditWindow(int? bookId = null)
    {
        InitializeComponent();
        _bookId = bookId;
        LoadComboBoxes();

        if (_bookId.HasValue)
        {
            WindowTitle.Text = "Редактировать книгу";
            Title = "Редактирование книги";
            LoadBook();
        }
    }

    private void LoadComboBoxes()
    {
        using var context = new LibraryContext();

        var authors = context.Authors.OrderBy(a => a.LastName).ToList();
        AuthorCombo.ItemsSource = authors;
        AuthorCombo.DisplayMemberPath = "FullName";
        AuthorCombo.SelectedValuePath = "Id";

        var genres = context.Genres.OrderBy(g => g.Name).ToList();
        GenreCombo.ItemsSource = genres;
        GenreCombo.DisplayMemberPath = "Name";
        GenreCombo.SelectedValuePath = "Id";
    }

    private void LoadBook()
    {
        using var context = new LibraryContext();
        var book = context.Books.Find(_bookId!.Value);
        if (book == null)
        {
            MessageBox.Show("Книга не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        TitleBox.Text = book.Title;
        AuthorCombo.SelectedValue = book.AuthorId;
        GenreCombo.SelectedValue = book.GenreId;
        YearBox.Text = book.PublishYear.ToString();
        IsbnBox.Text = book.ISBN;
        QuantityBox.Text = book.QuantityInStock.ToString();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            MessageBox.Show("Введите название книги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            TitleBox.Focus();
            return;
        }

        if (AuthorCombo.SelectedValue == null)
        {
            MessageBox.Show("Выберите автора.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (GenreCombo.SelectedValue == null)
        {
            MessageBox.Show("Выберите жанр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(YearBox.Text, out int year) || year < 0 || year > DateTime.Now.Year)
        {
            MessageBox.Show("Введите корректный год публикации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            YearBox.Focus();
            return;
        }

        if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
        {
            MessageBox.Show("Введите корректное количество (>= 0).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            QuantityBox.Focus();
            return;
        }

        using var context = new LibraryContext();

        Book book;
        if (_bookId.HasValue)
        {
            book = context.Books.Find(_bookId.Value)!;
        }
        else
        {
            book = new Book();
            context.Books.Add(book);
        }

        book.Title = TitleBox.Text.Trim();
        book.AuthorId = (int)AuthorCombo.SelectedValue;
        book.GenreId = (int)GenreCombo.SelectedValue;
        book.PublishYear = year;
        book.ISBN = IsbnBox.Text.Trim();
        book.QuantityInStock = quantity;

        context.SaveChanges();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
