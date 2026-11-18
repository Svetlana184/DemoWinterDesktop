using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiracleShoes1.Services;
using MiracleShoes1.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MiracleShoes1.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Window _window;

        private object? _currentView;
        private string _userInfo = string.Empty;

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string UserInfo
        {
            get => _userInfo;
            set => SetProperty(ref _userInfo, value);
        }

        public bool ShowOrdersBool => Session.IsAuthorized;

        public MainViewModel(Window window)
        {
            _window = window;

            if (Session.IsAuthorized)
            {
                var u = Session.CurrentUser!;
                var role = u.IdUserRoleNavigation?.NameUserRole ?? "?";
                UserInfo = $"Вы вошли как: {u.FullName} ({role})";
            }
            else
            {
                UserInfo = "Гостевой режим";
            }

            CurrentView = CreateCatalog();
        }

        private static CatalogView CreateCatalog() =>
            new() { DataContext = new CatalogViewModel() };

        private static OrdersView CreateOrders() =>
            new() { DataContext = new OrdersViewModel() };

        [RelayCommand]
        private void ShowCatalog() => CurrentView = CreateCatalog();

        [RelayCommand]
        private void ShowOrders()
        {
            if (Session.IsAuthorized)
                CurrentView = CreateOrders();
        }

        [RelayCommand]
        private void Logout()
        {
            Session.CurrentUser = null;
            new LoginWindow().Show();
            _window.Close();
        }
    }
}
