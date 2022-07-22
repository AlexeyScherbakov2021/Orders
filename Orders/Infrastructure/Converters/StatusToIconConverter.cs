using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Orders.Infrastructure.Converters
{
    internal class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage Source = null;

            if (value is int status)
            {

                switch ((EnumStatus)status)
                {
                    case EnumStatus.Created:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/создан.png", UriKind.Relative));
                        break;

                    case EnumStatus.ApprovWork:
                    case EnumStatus.CoordinateWork:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/рассмотр.png", UriKind.Relative));
                        break;

                    case EnumStatus.Approved:
                    case EnumStatus.Coordinated:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/рассмотрен.png", UriKind.Relative));
                        break;

                    case EnumStatus.Return:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/вернут.png", UriKind.Relative));
                        break;

                    case EnumStatus.Waiting:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/ожидание.png", UriKind.Relative));
                        break;

                    case EnumStatus.Refused:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/отказано.png", UriKind.Relative));
                        break;

                    case EnumStatus.Closed:
                        Source = new BitmapImage(new Uri("/Orders;component/Resource/закрыт.png", UriKind.Relative));
                        break;
                }



            }

            return Source;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
