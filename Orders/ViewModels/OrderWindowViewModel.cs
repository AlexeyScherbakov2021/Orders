using Microsoft.Win32;
using Orders.Infrastructure;
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


        public OrderWindowViewModel()
        {
        }


        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Отправить
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => true;
        private void OnSendCommandExecuted(object p)
        {

            CurrentStep.ro_check = 1;

            if (order.RouteOrders.All(it => it.ro_check == 1))
            {
                // маршрут окончен
                order.o_statusId = (int)EnumStatus.Approved;
            }
            else
            {
                order.o_stepRoute++;
                order.o_statusId = (int)EnumStatus.Coordinated;
            }

            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Обзор файлов
        //--------------------------------------------------------------------------------
        private readonly ICommand _BrowseCommand = null;
        public ICommand BrowseCommand => _BrowseCommand ?? new LambdaCommand(OnBrowseCommandExecuted, CanBrowseCommand);
        private bool CanBrowseCommand(object p) => true;
        private void OnBrowseCommandExecuted(object p)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();

            if (dlgOpen.ShowDialog() == true)
            {
                RouteAdding ra = new RouteAdding
                {
                    ad_text = dlgOpen.FileName,
                };

                CurrentStep.RouteAddings.Add(ra);
                CurrentStep.OnPropertyChanged(nameof(CurrentStep.RouteAddings));
            }
        }

        #endregion
    }
}
