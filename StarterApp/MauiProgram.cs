// Microsoft.Maui.Controls (Microsoft, 2024) - the cross-platform UI framework that powers the entire app
// it provides the Views, navigation, data binding, and platform-specific rendering for Android and Windows
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

            // Dependency Injection - API mode
            // The pattern here is AddSingleton<Interface, Implementation>()
            // This means: "whenever something asks for IAuthenticationService, give it ApiAuthenticationService"
            // The ViewModel never knows which implementation it gets - it just asks for the interface.
            // Originally this was LocalAuthenticationService - I changed it to ApiAuthenticationService
            // to connect to the shared class API instead of the local database.
            builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();

            // same pattern - ViewModels ask for IApiService, they get ApiService (the HTTP client)
            builder.Services.AddSingleton<IApiService, ApiService>();
            // ViewModels ask for IRentalService, they get ApiRentalService (talks to the shared API)
            builder.Services.AddSingleton<IRentalService, ApiRentalService>();
        }
        else
        {

            // this code came from the coursework example but it never runs because useSharedApi = true
            builder.Services.AddSingleton<IAuthenticationService, LocalAuthenticationService>(); // from the courswork


            // Dependency Injection - local database mode (runs when useSharedApi = false)
            // Same interface/implementation pattern but now pointing to local Repository classes
            // instead of the API. The ViewModels stay exactly the same - only this file changes.
            // This is the whole point of Dependency Injection - swap implementations without touching ViewModels.
            builder.Services.AddTransient<StarterApp.Database.Data.Repositories.IItemRepository, StarterApp.Database.Data.Repositories.ItemRepository>();
            builder.Services.AddTransient<StarterApp.Database.Data.Repositories.IRentalRepository, StarterApp.Database.Data.Repositories.RentalRepository>();
            builder.Services.AddTransient<StarterApp.Database.Data.Repositories.IReviewRepository, StarterApp.Database.Data.Repositories.ReviewRepository>();
            // in local mode IRentalService gets RentalService (talks to local DB) instead of ApiRentalService
            builder.Services.AddTransient<IRentalService, RentalService>();
        }

        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();





        // Registering my new Marketplace Views and ViewModels
        // AddTransient means a new instance is created every time the page is opened.
        // This is important for pages like RentalsPage where data needs to refresh each visit.
        // Each View and its ViewModel are registered as a pair - MAUI automatically
        // injects the ViewModel into the View's constructor when the page is navigated to.
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<EditItemViewModel>();
        builder.Services.AddTransient<EditItemPage>();
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