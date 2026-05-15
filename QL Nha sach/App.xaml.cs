using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QL_Nha_sach.Data;
using QL_Nha_sach.Pages;
using QL_Nha_sach.Services;
using QL_Nha_sach.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QL_Nha_sach
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // 1. Register the DbContextFactory
                    services.AddDbContextFactory<AppDbContext>(options =>
                        options.UseSqlite("Data Source=bookstore.db"));

                    services.AddSingleton<SessionManager>();

                    // Register InvoiceService
                    services.AddScoped<InvoiceService>();

                    // 2. Register your Windows and ViewModels
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<AddBookViewModel>();
                    services.AddTransient<PromotionViewModel>();
                    services.AddTransient<AddPromotionViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<EditBookViewModel>();
                    //services.AddTransient<EditPromotionViewModel>();
                    services.AddTransient<BookViewModel>();
                    services.AddTransient<HomeScreenViewModel>();
                    //services.AddTransient<ImportDetailViewModel>();
                    services.AddTransient<ImportListViewModel>();
                    services.AddTransient<ImportViewModel>();
                    //services.AddTransient<InvoiceDetailViewModel>();
                    services.AddTransient<InvoiceListViewModel>();
                    services.AddTransient<InvoiceViewModel>();
                    services.AddTransient<TransactionListViewModel>();
                    services.AddTransient<UserManagementViewModel>();
                    services.AddTransient<AddUserViewModel>();
                    //services.AddTransient<EditUserViewModel>();

                    services.AddTransient<AddBookPage>();
                    services.AddTransient<AddPromotionPage>();
                    services.AddTransient<BookManagementPage>();
                    //services.AddTransient<EditPromotionPage>();
                    services.AddTransient<LoginPage>();
                    services.AddTransient<ManagerHomePage>();
                    services.AddTransient<StaffHomePage>();
                    services.AddTransient<StockerHomePage>();
                    //services.AddTransient<ImportDetailPage>();
                    services.AddTransient<ImportListPage>();
                    //services.AddTransient<InvoiceDetailPage>();
                    services.AddTransient<InvoiceListPage>();
                    services.AddTransient<TransactionListPage>();
                    services.AddTransient<UserManagementPage>();
                    services.AddTransient<AddUserPage>();
                    //services.AddTransient<EditUserPage>();

                    //services.AddTransient<EditBookWindow>();
                    services.AddTransient<LookupWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            // Resolve the MainWindow from the DI container
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}
