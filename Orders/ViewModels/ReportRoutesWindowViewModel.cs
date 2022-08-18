using Orders.Infrastructure;
using Orders.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ViewModels
{
    internal class ReportRoutesWindowViewModel
    {
        public List<Order> ListOrder { get; set; }


        public ReportRoutesWindowViewModel()
        {
            ListOrder = MainWindowViewModel.repo.Orders
                .Where(it => it.o_statusId == (int)EnumStatus.Approved || it.RouteOrders
                    .Where(item => item.ro_check == EnumCheckedStatus.CheckedProcess)
                    .Any())
                .Include(ro => ro.RouteOrders.Select(s => s.User))
                .ToList();

            foreach(var item in ListOrder)
            {
                item.WorkUser = item.RouteOrders.FirstOrDefault(it => it.ro_step == item.o_stepRoute
                            && it.ro_check == EnumCheckedStatus.CheckedProcess)?.User;
            }
        }
    }
}
