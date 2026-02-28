using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement;

public partial class GenresWindow : Window
{
    private int? _selectedGenreId;

    public GenresWindow()
    {
        InitializeComponent();
        LoadGenres();
    }

    private void LoadGenres()
    {
        using var context = new LibraryContext();
        var genres = context.Genres
            .Include(g => g.Books)
            .OrderBy(g => g.Name)
            .ToList();
        GenresList.ItemsSource = genres;
    }

    private void GenresList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GenresList.SelectedItem is Genre genre)
        {
            _selectedGenreId = genre.Id;
            GenreNameBox.Text = genre.Name;
            DescriptionBox.Text = genre.Description;
        }
    }

    private void NewGenre_Click(object sender, RoutedEventArgs e)
    {
        _selectedGenreId = null;
        GenresList.SelectedItem = null;
        GenreNameBox.Text = "";
        DescriptionBox.Text = "";
        GenreNameBox.Focus();
    }

    private void SaveGenre_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GenreNameBox.Text))
        {
            MessageBox.Show("Введите название жанра.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new LibraryContext();

        Genre genre;
        if (_selectedGenreId.HasValue)
        {
            genre = context.Genres.Find(_selectedGenreId.Value)!;
        }
        else
        {
            genre = new Genre();
            context.Genres.Add(genre);
        }

        genre.Name = GenreNameBox.Text.Trim();
        genre.Description = DescriptionBox.Text.Trim();

        context.SaveChanges();
        _selectedGenreId = genre.Id;
        LoadGenres();

        // Re-select the saved genre
        if (GenresList.ItemsSource is List<Genre> list)
        {
            GenresList.SelectedItem = list.FirstOrDefault(g => g.Id == _selectedGenreId);
        }
    }

    private void DeleteGenre_Click(object sender, RoutedEventArgs e)
    {
        if (!_selectedGenreId.HasValue)
        {
            MessageBox.Show("Выберите жанр для удаления.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        using var context = new LibraryContext();
        var genre = context.Genres.Include(g => g.Books).First(g => g.Id == _selectedGenreId.Value);

        if (genre.Books.Any())
        {
            var result = MessageBox.Show(
                $"В жанре \"{genre.Name}\" есть {genre.Books.Count} книг(и). " +
                "При удалении жанра все эти книги тоже будут удалены. Продолжить?",
                "Внимание",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
        }
        else
        {
            var result = MessageBox.Show(
                $"Удалить жанр \"{genre.Name}\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;
        }

        context.Genres.Remove(genre);
        context.SaveChanges();

        _selectedGenreId = null;
        NewGenre_Click(sender, e);
        LoadGenres();
    }
}
