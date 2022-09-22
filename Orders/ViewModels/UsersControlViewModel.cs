using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using Orders.Wrapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class UsersControlViewModel : ViewModel
    {
        RepositoryBase repo;

        //public ObservableCollection<Role> ListRole { get; set; }





        private ObservableCollection<RoleUserWrap> _ListUser;
        public ObservableCollection<RoleUserWrap> ListUser
        {
            get => _ListUser;
            set
            {
                if (Set(ref _ListUser, value))
                {
                    _listUserViewSource = new CollectionViewSource();
                    _listUserViewSource.Source = value;
                    _listUserViewSource.View.Refresh();
                    ListUserView.CurrentChanged += ListUserView_CurrentChanged;
                }
            }
        }

        private void ListUserView_CurrentChanged(object sender, EventArgs e)
        {
            repo.Save();
        }

        CollectionViewSource _listUserViewSource;
        public ICollectionView ListUserView => _listUserViewSource?.View;

        public RoleUserWrap SelectedUser { get; set; }

        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Добавить 
        //--------------------------------------------------------------------------------
        public ICommand AddCommand => new LambdaCommand(OnAddCommandExecuted, CanAddCommand);
        private bool CanAddCommand(object p) => true;
        private void OnAddCommandExecuted(object p)
        {
            User newUser = new User { u_login = "Пользователь", u_role = 0 };
            //ListUser.Add(newUser);
            ListUserView.MoveCurrentToLast();
            repo.Add(newUser, true);
        }
        //--------------------------------------------------------------------------------
        // Команда Удалить
        //--------------------------------------------------------------------------------
        public ICommand DeleteCommand => new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommand);
        private bool CanDeleteCommand(object p) => SelectedUser != null;
        private void OnDeleteCommandExecuted(object p)
        {
            //if (MessageBox.Show($"Удалить {SelectedUser.u_login}", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    if (repo.Delete(SelectedUser, true))
            //        ListUser.Remove(SelectedUser);
            //}
        }

        //--------------------------------------------------------------------------------
        // Событие окончания редактирования ячейки
        //-------------------------------------------------------------------------------
        //public ICommand SelectOtdelCommand => new LambdaCommand(OnSelectOtdelCommandExecuted, CanSelectOtdelCommand);
        //private bool CanSelectOtdelCommand(object p) => true;
        //private void OnSelectOtdelCommandExecuted(object p)
        //{
        //    repoUser.Update(SelectedUser);
        //    ListUserView.Refresh();
        //}

        #endregion

        //--------------------------------------------------------------------------------
        // Конструктор 
        //--------------------------------------------------------------------------------
        public UsersControlViewModel() 
        {
            repo = SettingWindowViewModel.repo;
            var listUser = new ObservableCollection<User>(repo.Users.Include(it => it.RolesUser));
            ListUser = new ObservableCollection<RoleUserWrap>();

            foreach (var item in listUser)
            {
                RoleUserWrap wrap = new RoleUserWrap(item);
                ListUser.Add(wrap);
            }

            //ListRole = new ObservableCollection<Role>(repo.Roles);
            //OnPropertyChanged(nameof(ListRole));

        }
        //public UsersControlViewModel(RepositoryBase repoBase) : this()
        //{
        //    repo = repoBase;
        //    ListUser = new ObservableCollection<User>(repo.Users);
        //    ListRole = new ObservableCollection<Role>(repo.Roles);
        //    OnPropertyChanged(nameof(ListRole));
        //}

    }
}
