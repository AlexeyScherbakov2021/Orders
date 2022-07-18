using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ViewModels
{
    internal class CreateOrderWindowViewModel : ViewModel
    {
        public Order order { get; set; }
        private readonly RepositoryBase repoRoute = new RepositoryBase();
        public IEnumerable<Route> ListRoute { get; set; }


        public CreateOrderWindowViewModel()
        {
            ListRoute = repoRoute.Routes;
        }
    }
}
