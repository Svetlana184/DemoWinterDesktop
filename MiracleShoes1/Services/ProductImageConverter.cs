using MiracleShoes1.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MiracleShoes1.Services
{
    public class ProductImageConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int? imageId = values.Length > 0 && values[0] is int i && i > 0 ? i : null;
            string? url = values.Length > 1 ? values[1] as string : null;

            if (imageId.HasValue)
            {
                try
                {
                    using var ctx = new PostgresContext();
                    var data = ctx.ProductImages
                        .Where(x => x.IdProductImage == imageId.Value)
                        .Select(x => x.Data)
                        .FirstOrDefault();

                    if (data is { Length: > 0 })
                        return BytesToImage(data);
                }
                catch
                {
                    // fallback to url
                }
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch
                {
                    // ignore
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static BitmapImage BytesToImage(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
    }
}
