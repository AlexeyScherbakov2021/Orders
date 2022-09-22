using Orders.Repository;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Orders.ViewModels
{
    internal class SettingWindowViewModel : ViewModel
    {
        public static RepositoryBase repo = new RepositoryBase();

        public UsersControlViewModel UserViewModel { get; set; }
        public RouteControlViewModel RouteViewModel { get; set; }


        public SettingWindowViewModel()
        {
            UserViewModel = new UsersControlViewModel();
            RouteViewModel = new RouteControlViewModel(repo);

        }
    }
}
