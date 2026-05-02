using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        // Constructor: The 'factory' is injected by the code we put in App.xaml.cs
        public MainViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        private void LoadInitialData()
        {
            using var context = _factory.CreateDbContext();
            // Do your startup logic here
        }
    }
}
