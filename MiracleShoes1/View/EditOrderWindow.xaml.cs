using MiracleShoes1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MiracleShoes1.View
{
    /// <summary>
    /// Логика взаимодействия для EditOrderWindow.xaml
    /// </summary>
    public partial class EditOrderWindow : Window
    {
        private readonly Order _order;

        public DateTime? OrderDate { get; set; }

        public EditOrderWindow(Order order)
        {
            InitializeComponent();
            _order = order;
            OrderDate = order.DateOrder.ToDateTime(TimeOnly.MinValue);
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (OrderDate == null)
            {
                MessageBox.Show("Укажите дату заказа.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _order.DateOrder = DateOnly.FromDateTime(OrderDate.Value);
            DialogResult = true;
        }
    }
}
