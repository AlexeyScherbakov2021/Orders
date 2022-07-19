using Orders.Models;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ViewModels
{
    internal class OrderWindowViewModel : ViewModel
    {
        private Order _order;
        public Order order 
        { 
            get => _order; 
            set 
            { 
                if(Set(ref _order, value))
                {
                    CurrentStep = value.RouteOrders.FirstOrDefault(it => it.ro_step == value.o_stepRoute);
                }
            } 
        }
        
        private RouteOrder _CurrentStep;
        public RouteOrder CurrentStep { get => _CurrentStep; set { Set(ref _CurrentStep, value); } }

        


    }
}
