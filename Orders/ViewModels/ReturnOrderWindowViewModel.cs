using Orders.Models;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ViewModels
{
    internal class ReturnOrderWindowViewModel : ViewModel
    {
        public List<RouteOrder> ListRouteOrder { get; set; }
        public RouteOrder SelectedRouteOrder { get; set;}


        public ReturnOrderWindowViewModel(Order order)
        {
            ListRouteOrder = order.RouteOrders.Where(it => it.ro_step < order.o_stepRoute).ToList();

        }


    }
}
