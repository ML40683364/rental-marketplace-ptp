// this is the entry point for the whole app's setup.
// everything gets registered here - services, viewmodels, pages.
// the useSharedApi flag at the top controls whether we talk to the
// shared API or the local postgres database. changing it here is all you need to do.


// StarterApp original + coursework's useSharedApi block + marketplace additions

using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Views;
using System.Diagnostics;
using StarterApp.Services;

namespace StarterApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp() //Starts configuring app
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });







        // i have implemented this part of API from the courswork  -  Modification for MauiProgram.cs  

        // -------------------------------------------------------------------
        // Set useSharedApi to true to connect to the shared REST API,
        // or false to use the local PostgreSQL database.
        // This is an example of dependency injection — the ViewModels do
        // not change regardless of which implementation is registered here.
        // -------------------------------------------------------------------
        const bool useSharedApi = true; // has changed to be true to connect to the shared API instead of local database.

        // DbContext is always registered even in API mode because some
        // parts of the app still reference it during startup





        builder.Services.AddDbContext<AppDbContext>(); // i had to move AddDbContext outside of the else block so it always registers regardless of the flag.

        if (useSharedApi)

        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
            };

            builder.Services.AddSingleton(httpClient);
            // Originally, this was LocalAuthenticationService, but I changed it to ApiAuthenticationService to connect to the shared API instead of local database.
            builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();


            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<IRentalService, ApiRentalService>();
        }
        else
        {
            builder.Services.AddSingleton<IAuthenticationService, LocalAuthenticationService>(); // from the courswork


            // those part of the code are there for the local database mode — when useSharedApi = false
            builder.Services.AddTransient<StarterApp.Database.Data.Repositories.IItemRepository, StarterApp.Database.Data.Repositories.ItemRepository>();
            builder.Services.AddTransient<StarterApp.Database.Data.Repositories.IRentalRepository, StarterApp.Database.Data.Repositories.RentalRepository>();
            builder.Services.AddTransient<StarterApp.Database.Data.Repositories.IReviewRepository, StarterApp.Database.Data.Repositories.ReviewRepository>();
            builder.Services.AddTransient<IRentalService, RentalService>();
        }

        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();





        // added these when I built the rentals page                                                                                                   
        // not sure why this needs to be outside but it crashed without it
        // TODO: remove TempPage later  

        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddTransient<NearbyItemsPage>();
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();
        builder.Services.AddTransient<ReviewsViewModel>();
        builder.Services.AddTransient<ReviewsPage>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>(); // - basic - this is navigation shell
        builder.Services.AddSingleton<App>();

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>(); // basic - home screen
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>(); // basic - login form
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>(); // basic - register new account
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>(); // basic - list of users
        builder.Services.AddTransient<UserDetailPage>(); // basic - view a user's profile
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>(); // - basic - temporary/test page from the starter

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}