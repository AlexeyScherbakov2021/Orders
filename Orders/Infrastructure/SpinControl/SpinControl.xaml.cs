using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpinUpDown.SpinControl
{
    /// <summary>
    /// Логика взаимодействия для SpinControl.xaml
    /// </summary>
    public partial class SpinControl : UserControl
    {
        //--------------------------------------------------------------------------
        // Максимальное значение
        //--------------------------------------------------------------------------
        public static DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double), typeof(SpinControl),
            new FrameworkPropertyMetadata(100.0)
            );

        [Category ("Общие")]
        [Description ("Максимальное значнение")]
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty); 
            set => SetValue(MaximumProperty, value);
        }


        //--------------------------------------------------------------------------
        // Минимальное значение
        //--------------------------------------------------------------------------
        public static DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double), typeof(SpinControl),
            new FrameworkPropertyMetadata(0.0)
            );

        [Category("Общие")]
        [Description("Минимальное значнение")]
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }


        //--------------------------------------------------------------------------
        // Текущее значение
        //--------------------------------------------------------------------------
        public static DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(double), typeof(SpinControl),
            new FrameworkPropertyMetadata(0.0)
            );

        [Category("Общие")]
        [Description("Текущее значнение")]
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        //--------------------------------------------------------------------------
        // Добавление значения
        //--------------------------------------------------------------------------
        public static DependencyProperty LargeChangeProperty = DependencyProperty.Register(
            nameof(LargeChange), typeof(double), typeof(SpinControl),
            new FrameworkPropertyMetadata(1.0)
            );

        [Category("Общие")]
        [Description("Добавление значения")]
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }


        //--------------------------------------------------------------------------
        // Малое добавление значения
        //--------------------------------------------------------------------------
        public static DependencyProperty SmallChangeProperty = DependencyProperty.Register(
            nameof(SmallChange), typeof(double), typeof(SpinControl),
            new FrameworkPropertyMetadata(0.1)
            );

        [Category("Общие")]
        [Description("Малое добавление значения")]
        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }



        public SpinControl()
        {
            InitializeComponent();
        }

        private void UpValue_Click(object sender, RoutedEventArgs e)
        {
            double val = Value + LargeChange;

            if (val <= Maximum)
                Value = val;
        }

        private void DownValue_Click(object sender, RoutedEventArgs e)
        {
            double val = Value - LargeChange;

            if (val >= Minimum)
                Value = val;
        }


        private void SpinCtrl_KeyDown(object sender, KeyEventArgs e)
        {
            double val = 0;

            switch (e.Key)
            {
                case Key.Down:
                    val = Value - SmallChange;
                    if (val >= Minimum)
                        Value = val;
                    break;

                case Key.Up:
                    val = Value + SmallChange;
                    if (val <= Maximum)
                        Value = val;
                    break;

                case Key.PageDown:
                    val = Value - LargeChange;
                    if (val >= Minimum)
                        Value = val;
                    break;

                case Key.PageUp:
                    val = Value + LargeChange;
                    if (val <= Maximum)
                        Value = val;
                    break;

            }

        }



        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex _regex = new Regex("[0-9]");
            bool res = _regex.IsMatch(e.Text);

            TextBox tb = sender as TextBox;
            string s = tb.Text;
            if (tb.SelectionLength > 0)
            {
                s = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength);
            }
            s = s.Insert(tb.SelectionStart, (res ? e.Text : ""));

            double dbl;
            bool resDouble = double.TryParse(s, out dbl);

            if (resDouble)
            {
                res &= dbl >= Minimum && dbl <= Maximum;
            }

            e.Handled = !res;

        }

    }
}
