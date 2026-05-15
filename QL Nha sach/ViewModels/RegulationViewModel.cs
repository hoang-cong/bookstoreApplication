using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class RegulationViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public Regulation Regulation { get; private set; }

        public ICommand SaveCommand { get; }

        public RegulationViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadRegulation();
            SaveCommand = new RelayCommand(_ => SaveRegulation());
        }

        private void LoadRegulation()
        {
            using var context = _factory.CreateDbContext();
            Regulation = context.Regulations.FirstOrDefault() ?? new Regulation();
            OnPropertyChanged(nameof(Regulation));
        }

        private void SaveRegulation()
        {
            using var context = _factory.CreateDbContext();
            var existing = context.Regulations.FirstOrDefault();
            if (existing != null)
            {
                context.Entry(existing).CurrentValues.SetValues(Regulation);
            }
            else
            {
                context.Regulations.Add(Regulation);
            }
            context.SaveChanges();
            ShowMessage("Regulations saved successfully.", false);
        }
    }

}
