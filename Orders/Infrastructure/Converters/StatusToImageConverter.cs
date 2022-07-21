using FontAwesome5;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Orders.Infrastructure.Converters
{
    internal class StatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageAwesome img = null;


            if (value is int status)
            {
                //var n = FontAwesome();


                img = new ImageAwesome();

                switch ((EnumStatus)status)
                {
                    case EnumStatus.Created:
                        img.Icon = EFontAwesomeIcon.Solid_Home;
                        img.ToolTip = "Создан";
                        img.Foreground = Brushes.Green;
                        break;

                    case EnumStatus.ApprovWork:
                    case EnumStatus.CoordinateWork:
                        img.Icon = EFontAwesomeIcon.Regular_Edit;
                        img.ToolTip = ((EnumStatus)status) == EnumStatus.ApprovWork ? "На утверждении" : "На рассмотрении";
                        img.Foreground = Brushes.Green;
                        break;

                    case EnumStatus.Approved:
                    case EnumStatus.Coordinated:
                        img.Icon = EFontAwesomeIcon.Solid_Check;
                        img.ToolTip = ((EnumStatus)status) == EnumStatus.Approved ? "Утвержден" : "Рассмотрен";
                        img.Foreground = Brushes.Green;
                        break;

                    case EnumStatus.Return:
                        img.Icon = EFontAwesomeIcon.Solid_Backward;
                        img.ToolTip = "Возвращен";
                        img.Foreground = Brushes.DarkOrange;
                        break;

                    case EnumStatus.Waiting:
                        img.Icon = EFontAwesomeIcon.Regular_Hourglass;
                        img.ToolTip = "В ожидании рассмотрения";
                        img.Foreground = Brushes.Blue;
                        break;

                    case EnumStatus.Refused:
                        img.Icon = EFontAwesomeIcon.Solid_Ban;
                        img.ToolTip = "Отказано. Заказ снят.";
                        img.Foreground = Brushes.Red;
                        break;
                }
               
            }

            return img?.Icon;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
