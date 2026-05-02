using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Pages;
using QL_Nha_sach.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QL_Nha_sach
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var loginPage = App.AppHost.Services.GetRequiredService<LoginPage>();
            MainFrame.Navigate(loginPage);
        }
    }
}

/*
 * Yes — you can make the **login page the default** and then navigate to the correct role‑specific home page after successful login. Right now your `MainWindow` constructor hard‑codes navigation to `LoginPage`. That’s fine, but you need a way for the `LoginPage` (or its ViewModel) to tell `MainWindow` which page to go to once the user is authenticated.

---

### 🔑 Approach
1. **MainWindow** hosts a `Frame` (`MainFrame`) that can navigate to different pages.
2. **LoginPage** stays the default page.
3. After login, you check the user’s role and navigate to the correct home page (`ManagerHomePage`, `StaffHomePage`, `StockerHomePage`).
4. All home pages bind to the same `HomeScreenViewModel` (if you want one unified ViewModel).

---

### 🛠️ Example Flow

**MainWindow.xaml.cs**
```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Default page is login
        var loginPage = App.AppHost.Services.GetRequiredService<LoginPage>();
        loginPage.LoginSucceeded += OnLoginSucceeded; // subscribe to event
        MainFrame.Navigate(loginPage);
    }

    private void OnLoginSucceeded(User user)
    {
        // Decide which home page to show
        Page homePage;
        var factory = App.AppHost.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var homeVm = new HomeScreenViewModel(user, factory);

        switch (user.Role.RoleName)
        {
            case "Manager":
                homePage = new ManagerHomePage { DataContext = homeVm };
                break;
            case "Staff":
                homePage = new StaffHomePage { DataContext = homeVm };
                break;
            case "Stocker":
                homePage = new StockerHomePage { DataContext = homeVm };
                break;
            default:
                homePage = new DefaultHomePage { DataContext = homeVm };
                break;
        }

        MainFrame.Navigate(homePage);
    }
}
```

---

**LoginPage.xaml.cs**
```csharp
public partial class LoginPage : Page
{
    public event Action<User> LoginSucceeded;

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        using var context = App.AppHost.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
        var user = context.Users.Include(u => u.Role)
            .FirstOrDefault(u => u.Username == UsernameBox.Text && u.Password == PasswordBox.Password);

        if (user != null)
        {
            LoginSucceeded?.Invoke(user);
        }
        else
        {
            MessageBox.Show("Invalid credentials.");
        }
    }
}
```

---

### 🚀 Benefits
- **Login is always the default page** (your `MainWindow` starts with it).
- **Navigation is centralized** in `MainWindow` — it decides which home page to show after login.
- **Clean MVVM**: you can still bind each home page to the same `HomeScreenViewModel`, but the XAML differs per role.

---

👉 So yes, you can keep `LoginPage` as the default, and then use an event or callback from the login process to tell `MainWindow` which role‑specific page to navigate to.  

Would you like me to sketch out a **single unified HomeScreenViewModel** that exposes role‑specific flags/commands, so you don’t need separate Manager/Staff/Stocker ViewModels? That would make the binding even simpler.
 public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var loginPage = App.AppHost.Services.GetRequiredService<LoginPage>();
        loginPage.LoginSucceeded += OnLoginSucceeded;
        MainFrame.Navigate(loginPage);
    }

    private void OnLoginSucceeded(User user)
    {
        var factory = App.AppHost.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var homeVm = new HomeScreenViewModel(user, factory);

        Page homePage;
        switch (user.Role.RoleName)
        {
            case "Manager":
                homePage = new ManagerHomePage { DataContext = homeVm };
                break;
            case "Staff":
                homePage = new StaffHomePage { DataContext = homeVm };
                break;
            case "Stocker":
                homePage = new StockerHomePage { DataContext = homeVm };
                break;
            default:
                homePage = new DefaultHomePage { DataContext = homeVm };
                break;
        }

        MainFrame.Navigate(homePage);
    }
}

 */