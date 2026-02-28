using System.Windows;
using LibraryManagement.Data;

namespace LibraryManagement;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure database is created and seeded
        using var context = new LibraryContext();
        context.Database.EnsureCreated();
        LibraryContext.SeedData(context);
    }
}
