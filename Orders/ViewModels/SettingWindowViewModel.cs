using Orders.Repository;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ViewModels
{
    internal class SettingWindowViewModel : ViewModel
    {
        public static RepositoryBase repo = new RepositoryBase();


        public SettingWindowViewModel()
        {

        }
    }
}
