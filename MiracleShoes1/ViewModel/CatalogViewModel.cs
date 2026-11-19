using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoes1.Model;
using MiracleShoes1.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MiracleShoes1.ViewModel
{
    public partial class CatalogViewModel : ObservableObject
    {
        private readonly PostgresContext _context;

        private ObservableCollection<ProductListItem> _products = new();
        private string _searchText = string.Empty;
        private string _selectedSort = string.Empty;

        public ObservableCollection<ProductListItem> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string SelectedSort
        {
            get => _selectedSort;
            set => SetProperty(ref _selectedSort, value);
        }

        public bool CanUseFilters => Session.IsAuthorized;
        public bool CanOrder => Session.IsAuthorized;

        public string[] SortOptions { get; } =
            { "По умолчанию", "Цена (возр.)", "Цена (убыв.)", "Название" };

        public CatalogViewModel()
        {
            _context = new PostgresContext();
            SelectedSort = SortOptions[0];
            LoadProducts();
        }

        [RelayCommand]
        private void LoadProducts()
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (CanUseFilters && !string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.ToLower();
                query = query.Where(p => p.NameProduct.ToLower().Contains(s));
            }

            if (CanUseFilters)
            {
                query = SelectedSort switch
                {
                    "Цена (возр.)" => query.OrderBy(p => p.Cost),
                    "Цена (убыв.)" => query.OrderByDescending(p => p.Cost),
                    "Название" => query.OrderBy(p => p.NameProduct),
                    _ => query
                };
            }

            var list = query.Select(p => new ProductListItem
            {
                IdProduct = p.IdProduct,
                NameProduct = p.NameProduct,
                Description = p.Description,
                Cost = p.Cost,
                Category = p.Category,
                Subcategory = p.Subcategory,
                CompanyName = p.IdCompanyNavigation == null
                    ? null
                    : p.IdCompanyNavigation.NameCompany,
                //PrimaryImageId = p.ProductImage
                //    .Where(i => i.IsPrimary)
                //    .Select(i => (int?)i.IdProductImage)
                //    .FirstOrDefault(),
                ExternalUrl = p.Url,
                Sizes = p.AvailableSizes
                    .Where(a => a.IsAvailable && a.Amount > 0)
                    .Select(a => new SizeOption
                    {
                        IdSize = a.IdSize,
                        ShoeSize = a.IdSizeNavigation.ShoeSize,
                        Amount = a.Amount
                    })
                    .ToList()
            }).ToList();

            Products = new ObservableCollection<ProductListItem>(list);
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void AddToOrder(ProductListItem? item)
        {
            if (item == null)
                return;

            if (item.Sizes.Count == 0)
            {
                MessageBox.Show("Нет доступных размеров для заказа.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var size = item.Sizes.First();

            var avail = _context.AvailableSizes
                .FirstOrDefault(a => a.IdProduct == item.IdProduct && a.IdSize == size.IdSize);

            if (avail == null || avail.Amount <= 0)
            {
                MessageBox.Show("Нет в наличии.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var userId = Session.CurrentUser!.IdUser;

            var order = _context.Orders
                .FirstOrDefault(o => o.IdUser == userId && o.DateOrder == today);

            if (order == null)
            {
                order = new Order { IdUser = userId, DateOrder = today };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            var existing = _context.OrderItems
                .FirstOrDefault(i => i.IdOrder == order.IdOrder
                                     && i.IdProduct == item.IdProduct
                                     && i.IdSize == size.IdSize);

            if (existing != null)
            {
                existing.Amount += 1;
                existing.CostForItem = item.Cost;
            }
            else
            {
                _context.OrderItems.Add(new OrderItem
                {
                    IdOrder = order.IdOrder,
                    IdProduct = item.IdProduct,
                    IdSize = size.IdSize,
                    Amount = 1,
                    CostForItem = item.Cost
                });
            }

            avail.Amount -= 1;
            if (avail.Amount <= 0)
                avail.IsAvailable = false;

            _context.SaveChanges();
            LoadProducts();

            MessageBox.Show("Товар добавлен в заказ.", "Заказ",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class ProductListItem
    {
        public int IdProduct { get; set; }
        public string NameProduct { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Cost { get; set; }
        public int Amount { get; set; }
        public string Category { get; set; } = null!;
        public string? Subcategory { get; set; }
        public string? CompanyName { get; set; }
        public int? PrimaryImageId { get; set; }
        public string? ExternalUrl { get; set; }
        public List<SizeOption> Sizes { get; set; } = new();
    }

    public class SizeOption
    {
        public int IdSize { get; set; }
        public double ShoeSize { get; set; }
        public int Amount { get; set; }
    }
}
