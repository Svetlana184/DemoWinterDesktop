using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoes1.Model;
using MiracleShoes1.Services;
using MiracleShoes1.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MiracleShoes1.ViewModel
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly PostgresContext _context;

        private ObservableCollection<Order> _orders = new();
        private Order? _selectedOrder;
        private ObservableCollection<OrderItem> _selectedItems = new();

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    SelectedItems = value!= null
                        ? new ObservableCollection<OrderItem>()
                        : new ObservableCollection<OrderItem>();
                }
            }
        }

        public ObservableCollection<OrderItem> SelectedItems
        {
            get => _selectedItems;
            set => SetProperty(ref _selectedItems, value);
        }

        public bool CanDelete => Session.IsManager || Session.IsAdmin;
        public bool CanEdit => Session.IsAdmin;
        public bool CanOrder => Session.IsAuthorized;

        public OrdersViewModel()
        {
            _context = new PostgresContext();
            LoadOrders();
        }

        private void LoadOrders()
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.IdUserNavigation)
                //.Include(o => o.Items).ThenInclude(i => i.IdProductNavigation)
                //.Include(o => o.Items).ThenInclude(i => i.IdSizeNavigation)
                .AsQueryable();

            if (!Session.IsManager && !Session.IsAdmin && Session.CurrentUser != null)
                query = query.Where(o => o.IdUser == Session.CurrentUser.IdUser);

            var list = query
                .OrderByDescending(o => o.DateOrder)
                .ThenByDescending(o => o.IdOrder)
                .ToList();

            Orders = new ObservableCollection<Order>(list);

            if (SelectedOrder != null)
            {
                var refreshed = Orders.FirstOrDefault(o => o.IdOrder == SelectedOrder.IdOrder);
                SelectedOrder = refreshed;
            }
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void DeleteOrder(Order? order)
        {
            if (order == null)
                return;

            var result = MessageBox.Show(
                $"Удалить заказ №{order.IdOrder}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            using var ctx = new PostgresContext();
            var entity = ctx.Orders.FirstOrDefault(o => o.IdOrder == order.IdOrder);
            if (entity != null)
            {
                ctx.Orders.Remove(entity);
                ctx.SaveChanges();
            }

            LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private void EditOrder(Order? order)
        {
            if (order == null)
                return;

            var win = new EditOrderWindow(order);
            if (win.ShowDialog() == true)
            {
                using var ctx = new PostgresContext();
                var entity = ctx.Orders.FirstOrDefault(o => o.IdOrder == order.IdOrder);
                if (entity != null)
                {
                    entity.DateOrder = order.DateOrder;
                    ctx.SaveChanges();
                }
                LoadOrders();
            }
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void RemoveItem(OrderItem? item)
        {
            if (item == null)
                return;

            using var ctx = new PostgresContext();

            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null)
                return;

            if (entity.IdSize.HasValue)
            {
                var avail = ctx.AvailableSizes
                    .FirstOrDefault(a => a.IdProduct == entity.IdProduct && a.IdSize == entity.IdSize);
                if (avail != null)
                {
                    avail.Amount += entity.Amount;
                    avail.IsAvailable = true;
                }
            }

            var orderId = entity.IdOrder;
            ctx.OrderItems.Remove(entity);

            var left = ctx.OrderItems.Count(i => i.IdOrder == orderId) - 1;
            if (left == 0)
            {
                var order = ctx.Orders.FirstOrDefault(o => o.IdOrder == orderId);
                if (order != null)
                    ctx.Orders.Remove(order);
            }

            ctx.SaveChanges();
            LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void IncreaseItem(OrderItem? item)
        {
            if (item == null)
                return;

            using var ctx = new PostgresContext();

            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null)
                return;

            var avail = entity.IdSize.HasValue
                ? ctx.AvailableSizes.FirstOrDefault(a =>
                    a.IdProduct == entity.IdProduct && a.IdSize == entity.IdSize)
                : null;

            if (avail == null || avail.Amount <= 0)
            {
                MessageBox.Show("Нет в наличии.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            entity.Amount += 1;
            avail.Amount -= 1;
            if (avail.Amount <= 0)
                avail.IsAvailable = false;

            ctx.SaveChanges();
            LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void DecreaseItem(OrderItem? item)
        {
            if (item == null)
                return;

            if (item.Amount <= 1)
            {
                RemoveItem(item);
                return;
            }

            using var ctx = new PostgresContext();

            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null)
                return;

            entity.Amount -= 1;

            if (entity.IdSize.HasValue)
            {
                var avail = ctx.AvailableSizes
                    .FirstOrDefault(a => a.IdProduct == entity.IdProduct && a.IdSize == entity.IdSize);
                if (avail != null)
                {
                    avail.Amount += 1;
                    avail.IsAvailable = true;
                }
            }

            ctx.SaveChanges();
            LoadOrders();
        }
    }
}
