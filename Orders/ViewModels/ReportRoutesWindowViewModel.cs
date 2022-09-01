using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class ReportRoutesWindowViewModel : ViewModel
    {
        private string _FilterName = "";
        public string FilterName { get => _FilterName; set { if(Set(ref _FilterName, value)) { _ListOrderView.View.Refresh(); }}}

        private string _FilterBuyer = "";
        public string FilterByuer { get => _FilterBuyer; set { if (Set(ref _FilterBuyer, value)) { _ListOrderView.View.Refresh(); } } }

        private string _FilterCardOrder = "";
        public string FilterCardOrder { get => _FilterCardOrder; set { if (Set(ref _FilterCardOrder, value)) { _ListOrderView.View.Refresh(); } } }

        public List<Order> ListOrder { get; set; }

        public CollectionViewSource _ListOrderView { get; set; } = new CollectionViewSource();

        public ReportRoutesWindowViewModel()
        {
            ListOrder = MainWindowViewModel.repo.Orders
                .Where(it => it.o_statusId < EnumStatus.Closed )
                .Include(ro => ro.RouteOrders.Select(s => s.User))
                .ToList();

            _ListOrderView.Source = ListOrder;
            _ListOrderView.Filter += _ListOrderView_Filter;

            //ListOrder = MainWindowViewModel.repo.Orders
            //    .Where(it => it.o_statusId == EnumStatus.Approved || it.RouteOrders
            //        .Where(item => item.ro_check == EnumCheckedStatus.CheckedProcess)
            //        .Any())
            //    .Include(ro => ro.RouteOrders.Select(s => s.User))
            //    .ToList();

            foreach (var item in ListOrder)
            {
                item.WorkUser = item.RouteOrders.FirstOrDefault(it => it.ro_step == item.o_stepRoute
                            && it.ro_check == EnumCheckedStatus.CheckedProcess)?.User;
            }
        }

        private void _ListOrderView_Filter(object sender, FilterEventArgs e)
        {
            if(e.Item is Order order )
            {
                string Name = order.o_name ?? "";
                string Byuer = order.o_buyer ?? "";
                string CardOrder = order.o_CardOrder ?? "";

                if (!Name.ToLower().Contains(FilterName.ToLower())
                    || !Byuer.ToLower().Contains(FilterByuer.ToLower())
                    || !CardOrder.ToLower().Contains(FilterCardOrder.ToLower())
                    )
                    e.Accepted = false;
            }

            //e.Accepted = false;
        }

        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Обновить
        //--------------------------------------------------------------------------------
        private readonly ICommand _ClearFilterCommand = null;
        public ICommand ClearFilterCommand => _ClearFilterCommand ?? new LambdaCommand(OnClearFilterCommandExecuted, CanClearFilterCommand);
        private bool CanClearFilterCommand(object p) => true;
        private void OnClearFilterCommandExecuted(object p)
        {
            if(p is TextBox tb)
            {
                tb.Text = "";
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Фильтр по тексту
        //--------------------------------------------------------------------------------
        private readonly ICommand _FilterCommand = null;
        public ICommand FilterCommand => _FilterCommand ?? new LambdaCommand(OnClearFilterCommandExecuted, CanClearFilterCommand);
        private bool CanFilterCommand(object p) => true;
        private void OnFilterCommandExecuted(object p)
        {

        }

        #endregion
    }
}
