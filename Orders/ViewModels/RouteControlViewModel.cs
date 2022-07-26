using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class RouteControlViewModel : ViewModel
    {

        RepositoryBase repo; // = new RepositoryBase();

        public ObservableCollection<User> ListUser { get; set; }
        public ObservableCollection<RouteType> ListRouteType { get; set; }

        private ObservableCollection<Route> _ListRoute;
        public ObservableCollection<Route> ListRoute
        {
            get => _ListRoute;
            set
            {
                if (Set(ref _ListRoute, value))
                {
                    _listRouteViewSource = new CollectionViewSource();
                    _listRouteViewSource.Source = value;
                    _listRouteViewSource.View.Refresh();
                    ListRouteView.CurrentChanged += ListRouteView_CurrentChanged;
                }
            }
        }

        CollectionViewSource _listRouteViewSource;
        public ICollectionView ListRouteView => _listRouteViewSource?.View;


        public CollectionViewSource _ListRouteStepView { get; set; } = new CollectionViewSource();

        private Route _SelectedRoute;
        public Route SelectedRoute 
        { 
            get => _SelectedRoute; 
            set 
            { 
                if(Set(ref _SelectedRoute, value) && value != null)
                {
                    _ListRouteStepView.Source = value.RouteSteps;
                    _ListRouteStepView.View.SortDescriptions.Add(new SortDescription { PropertyName = "r_step", Direction = ListSortDirection.Ascending });
                }
            } 
        }

        public RouteStep SelectedStep { get; set; }
        public User SelectAddUser { get; set; }


        private void ListRouteView_CurrentChanged(object sender, EventArgs e)
        {
            repo.Save();
        }


        public RouteControlViewModel(RepositoryBase repoBase)
        {
            repo = repoBase;
            ListUser = new ObservableCollection<User>( repo.Users.Where(it => it.u_role < 1));
            ListRouteType = new ObservableCollection<RouteType>( repo.RouteTypes.Where(it => it.id != (int)EnumTypesStep.Created));
            ListRouteType[0].IsCheck = true;
            ListRoute = new ObservableCollection<Route>(repo.Routes);

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
            Route route = new Route { r_name = "Новый маршрут" };
            if (repo.Add(route))
            {
                ListRoute.Add(route);
                ListRouteView.MoveCurrentToLast();
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Удалить маршрут
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteCommand = null;
        public ICommand DeleteCommand => _DeleteCommand ?? new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommand);
        private bool CanDeleteCommand(object p) => SelectedRoute != null;
        private void OnDeleteCommandExecuted(object p)
        {
            //repo.Delete(SelectedRoute, true);

            if (repo.Delete(SelectedRoute, true))
            {
                //SelectedRoute.RouteSteps.Clear();
                ListRoute.Remove(SelectedRoute);
            }
            //repoRoute.Save();

        }

        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddStepCommand = null;
        public ICommand AddStepCommand => _AddStepCommand ?? new LambdaCommand(OnAddStepCommandExecuted, CanAddStepCommand);
        private bool CanAddStepCommand(object p) => SelectedRoute != null && SelectAddUser != null;
        private void OnAddStepCommandExecuted(object p)
        {
            int LastStep = SelectedRoute.RouteSteps.Count > 0 
                ? SelectedRoute.RouteSteps.Max(it => it.r_step) + 1
                : 1;

            RouteStep step = new RouteStep
            {
                r_step = LastStep,
                r_disabled = false,
                r_userId = SelectAddUser.id,
                r_type = ListRouteType.Single(it => it.IsCheck == true).id
            };

            SelectedRoute.RouteSteps.Add(step);
            repo.Save();

        }

        //--------------------------------------------------------------------------------
        // Команда Удалить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteStepCommand = null;
        public ICommand DeleteStepCommand => _DeleteStepCommand ?? new LambdaCommand(OnDeleteStepCommandExecuted, CanDeleteStepCommand);
        private bool CanDeleteStepCommand(object p) => SelectedStep != null;
        private void OnDeleteStepCommandExecuted(object p)
        {

            repo.DeleteRouteStep(SelectedStep);
            Renumerate();
            repo.Save();
        }

        //--------------------------------------------------------------------------------
        // Команда Обновить
        //--------------------------------------------------------------------------------
        private readonly ICommand _RefreshCommand = null;
        public ICommand RefreshCommand => _RefreshCommand ?? new LambdaCommand(OnRefreshCommandExecuted/*, CanRefreshCommand*/);
        //private bool CanRefreshCommand(object p) => SelectedStep != null;
        private void OnRefreshCommandExecuted(object p)
        {
            ListUser = new ObservableCollection<User>(repo.Users.Where(it => it.u_role < 1));
            OnPropertyChanged(nameof(ListUser));
        }

        //--------------------------------------------------------------------------------
        // Команда Этап выше
        //--------------------------------------------------------------------------------
        private readonly ICommand _StepUpCommand = null;
        public ICommand StepUpCommand => _StepUpCommand ?? new LambdaCommand(OnStepUpCommandExecuted, CanStepUpCommand);
        private bool CanStepUpCommand(object p) => SelectedStep?.r_step > 1;
        private void OnStepUpCommandExecuted(object p)
        {
            RouteStep step = SelectedRoute.RouteSteps.FirstOrDefault(it => it.r_step == SelectedStep.r_step - 1);
            int index = SelectedRoute.RouteSteps.IndexOf(step) + 1;
            step.r_step++;
            SelectedStep.r_step--;

            SelectedRoute.RouteSteps.Remove(step);
            SelectedRoute.RouteSteps.Insert(index, step);

            step.OnPropertyChanged(nameof(step.r_step));
            SelectedStep.OnPropertyChanged(nameof(SelectedStep.r_step));
            repo.Save();
        }

        //--------------------------------------------------------------------------------
        // Команда Этап ниже
        //--------------------------------------------------------------------------------
        private readonly ICommand _StepDownCommand = null;
        public ICommand StepDownCommand => _StepDownCommand ?? new LambdaCommand(OnStepDownCommandExecuted, CanStepDownCommand);
        private bool CanStepDownCommand(object p) => SelectedStep?.r_step < SelectedRoute?.RouteSteps?.Count;
        private void OnStepDownCommandExecuted(object p)
        {
            RouteStep step = SelectedRoute.RouteSteps.FirstOrDefault(it => it.r_step == SelectedStep.r_step + 1);
            int index = SelectedRoute.RouteSteps.IndexOf(step) - 1;
            step.r_step--;
            SelectedStep.r_step++;

            SelectedRoute.RouteSteps.Remove(step);
            SelectedRoute.RouteSteps.Insert(index, step);

            step.OnPropertyChanged(nameof(step.r_step));
            SelectedStep.OnPropertyChanged(nameof(SelectedStep.r_step));
            repo.Save();
        }

        #endregion


        //--------------------------------------------------------------------------------
        // Перенумерация этапов
        //--------------------------------------------------------------------------------
        private void Renumerate()
        {
            int i = 1;
            foreach (var item in SelectedRoute.RouteSteps)
            {
                item.r_step = i++;
                item.OnPropertyChanged(nameof(item.r_step));
            }
        }


    }
}
