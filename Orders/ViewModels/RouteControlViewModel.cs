using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class RouteControlViewModel : ViewModel
    {
        IRepository<Route> repoRoute = new RepositoryMain<Route>();
        IRepository<User> repoUser = new RepositoryMain<User>();
        IRepository<RouteType> repoRouteType = new RepositoryMain<RouteType>();

        public IEnumerable<User> ListUser { get; set; }
        public IEnumerable<RouteType> ListRouteType { get; set; }
        public List<Route> ListRoute { get; set; }
        public Route SelectedRoute { get; set; }


        public RouteControlViewModel()
        {
            ListUser = repoUser.Items;
            ListRouteType = repoRouteType.Items;

            ListRoute = repoRoute.Items.ToList();
        }



        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Добавить маршрут
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddCommand = null;
        public ICommand AddCommand => _AddCommand ?? new LambdaCommand(OnAddCommandExecuted, CanAddCommand);
        private bool CanAddCommand(object p) => true;
        private void OnAddCommandExecuted(object p)
        {

        }

        //--------------------------------------------------------------------------------
        // Команда Удалить маршрут
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteCommand = null;
        public ICommand DeleteCommand => _DeleteCommand ?? new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommand);
        private bool CanDeleteCommand(object p) => true;
        private void OnDeleteCommandExecuted(object p)
        {

        }

        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddStepCommand = null;
        public ICommand AddStepCommand => _AddStepCommand ?? new LambdaCommand(OnAddStepCommandExecuted, CanAddStepCommand);
        private bool CanAddStepCommand(object p) => true;
        private void OnAddStepCommandExecuted(object p)
        {

        }

        //--------------------------------------------------------------------------------
        // Команда Удалить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteStepCommand = null;
        public ICommand DeleteStepCommand => _DeleteStepCommand ?? new LambdaCommand(OnDeleteStepCommandExecuted, CanDeleteStepCommand);
        private bool CanDeleteStepCommand(object p) => true;
        private void OnDeleteStepCommandExecuted(object p)
        {

        }

        //--------------------------------------------------------------------------------
        // Команда Этап выше
        //--------------------------------------------------------------------------------
        private readonly ICommand _StepUpCommand = null;
        public ICommand StepUpCommand => _StepUpCommand ?? new LambdaCommand(OnStepUpCommandExecuted, CanStepUpCommand);
        private bool CanStepUpCommand(object p) => true;
        private void OnStepUpCommandExecuted(object p)
        {

        }

        //--------------------------------------------------------------------------------
        // Команда Этап ниже
        //--------------------------------------------------------------------------------
        private readonly ICommand _StepDownCommand = null;
        public ICommand StepDownCommand => _StepDownCommand ?? new LambdaCommand(OnStepDownCommandExecuted, CanStepDownCommand);
        private bool CanStepDownCommand(object p) => true;
        private void OnStepDownCommandExecuted(object p)
        {

        }



        #endregion



    }
}
