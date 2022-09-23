﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Orders.Infrastructure
{
    internal class DataGridScrollSelect
    {
        public static readonly DependencyProperty SelectingItemProperty = DependencyProperty.RegisterAttached(
              "SelectingItem",
              typeof(object),
              typeof(DataGridScrollSelect),
              new PropertyMetadata(default(object), OnSelectingItemChanged));

        public static object GetSelectingItem(DependencyObject target)
        {
            return (object)target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, object value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItem == null)
                return;

            // Works with .Net 4.5
            grid.Dispatcher.InvokeAsync(() =>
            {
                if (grid.SelectedItem != null)
                {
                    grid.UpdateLayout();
                    grid.ScrollIntoView(grid.SelectedItem, null);
                }
            });

            // Works with .Net 4.0
            grid.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (grid.SelectedItem != null)
                {
                    grid.UpdateLayout();
                    grid.ScrollIntoView(grid.SelectedItem, null);
                }
                
            }));
        }
    }
}
