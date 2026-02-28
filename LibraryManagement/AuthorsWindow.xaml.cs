using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement;

public partial class AuthorsWindow : Window
{
    private int? _selectedAuthorId;

    public AuthorsWindow()
    {
        InitializeComponent();
        LoadAuthors();
    }

    private void LoadAuthors()
    {
        using var context = new LibraryContext();
        var authors = context.Authors
            .Include(a => a.Books)
            .OrderBy(a => a.LastName)
            .ToList();
        AuthorsList.ItemsSource = authors;
    }

    private void AuthorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AuthorsList.SelectedItem is Author author)
        {
            _selectedAuthorId = author.Id;
            FirstNameBox.Text = author.FirstName;
            LastNameBox.Text = author.LastName;
            BirthDatePicker.SelectedDate = author.BirthDate;
            CountryBox.Text = author.Country;
        }
    }

    private void NewAuthor_Click(object sender, RoutedEventArgs e)
    {
        _selectedAuthorId = null;
        AuthorsList.SelectedItem = null;
        FirstNameBox.Text = "";
        LastNameBox.Text = "";
        BirthDatePicker.SelectedDate = null;
        CountryBox.Text = "";
        FirstNameBox.Focus();
    }

    private void SaveAuthor_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || string.IsNullOrWhiteSpace(LastNameBox.Text))
        {
            MessageBox.Show("Введите имя и фамилию автора.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new LibraryContext();

        Author author;
        if (_selectedAuthorId.HasValue)
        {
            author = context.Authors.Find(_selectedAuthorId.Value)!;
        }
        else
        {
            author = new Author();
            context.Authors.Add(author);
        }

        author.FirstName = FirstNameBox.Text.Trim();
        author.LastName = LastNameBox.Text.Trim();
        author.BirthDate = BirthDatePicker.SelectedDate;
        author.Country = CountryBox.Text.Trim();

        context.SaveChanges();
        _selectedAuthorId = author.Id;
        LoadAuthors();

        // Re-select the saved author
        if (AuthorsList.ItemsSource is List<Author> list)
        {
            AuthorsList.SelectedItem = list.FirstOrDefault(a => a.Id == _selectedAuthorId);
        }
    }

    private void DeleteAuthor_Click(object sender, RoutedEventArgs e)
    {
        if (!_selectedAuthorId.HasValue)
        {
            MessageBox.Show("Выберите автора для удаления.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        using var context = new LibraryContext();
        var author = context.Authors.Include(a => a.Books).First(a => a.Id == _selectedAuthorId.Value);

        if (author.Books.Any())
        {
            var result = MessageBox.Show(
                $"У автора \"{author.FullName}\" есть {author.Books.Count} книг(и). " +
                "При удалении автора все его книги тоже будут удалены. Продолжить?",
                "Внимание",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
        }
        else
        {
            var result = MessageBox.Show(
                $"Удалить автора \"{author.FullName}\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;
        }

        context.Authors.Remove(author);
        context.SaveChanges();

        _selectedAuthorId = null;
        NewAuthor_Click(sender, e);
        LoadAuthors();
    }
}
