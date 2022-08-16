using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.ViewModels.Base;
using Orders.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class ReturnOrderWindowViewModel : ViewModel
    {
        public List<RouteOrder> ListRouteOrder { get; set; }
        public RouteOrder SelectedRouteOrder { get; set;}
        public string ReturnMessage { get; set; }

        public ReturnOrderWindowViewModel(Order order)
        {
            ListRouteOrder = order.RouteOrders.Where(it => it.ro_step < order.o_stepRoute).ToList();

        }

        #region Команды


        //--------------------------------------------------------------------------------
        // Команда Создать
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReturnCommand = null;
        public ICommand ReturnCommand => _ReturnCommand ?? new LambdaCommand(OnReturnCommandExecuted, CanReturnCommand);
        private bool CanReturnCommand(object p) => SelectedRouteOrder != null;
        private void OnReturnCommandExecuted(object p)
        {

            App.Current.Windows.OfType<ReturnOrderWindow>().FirstOrDefault().DialogResult = true;
        }

        #endregion

    }
}
