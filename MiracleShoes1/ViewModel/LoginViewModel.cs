using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoes1.Model;
using MiracleShoes1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MiracleShoes1.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly PostgresContext _context;
        private readonly Window _loginWindow;

        private string? _selectedLogin;

        public string? SelectedLogin
        {
            get => _selectedLogin;
            set => SetProperty(ref _selectedLogin, value);
        }

        public string[] AvailableLogins { get; }

        public LoginViewModel(Window loginWindow)
        {
            _loginWindow = loginWindow;
            _context = new PostgresContext();
            AvailableLogins = _context.Users
                .OrderBy(u => u.Login)
                .Select(u => u.Login)
                .ToArray();
        }

        [RelayCommand]
        private void Login()
        {
            if (string.IsNullOrWhiteSpace(SelectedLogin))
            {
                MessageBox.Show("Выберите логин.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _context.Users
                .Include(u => u.IdUserRoleNavigation)
                .FirstOrDefault(u => u.Login == SelectedLogin);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден в системе.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Session.CurrentUser = user;
            var main = new MainWindow();
            main.Show();
            _loginWindow.Close();
        }
    }
}
