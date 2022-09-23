﻿using Orders.Infrastructure.Commands;
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

        private string _Filter = "";
        public string Filter { get => _Filter; set { if (Set(ref _Filter, value)) { _listUserViewSource.View.Refresh(); } } }


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
                    _listUserViewSource.Filter += _ListView_Filter;
                    //ListUserView.CurrentChanged += ListUserView_CurrentChanged;
                }
            }
        }


        private void _ListView_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is RoleUserWrap UserWrap)
            {
                if (!UserWrap.User.u_login.ToLower().StartsWith(Filter.ToLower())
                    && !UserWrap.User.u_name.ToLower().StartsWith(Filter.ToLower()))
                    e.Accepted = false;
            }

            //e.Accepted = false;
        }


        //private void ListUserView_CurrentChanged(object sender, EventArgs e)
        //{
        //}

        CollectionViewSource _listUserViewSource;
        public ICollectionView ListUserView => _listUserViewSource?.View;

        private RoleUserWrap _SelectedUser;
        public RoleUserWrap SelectedUser 
        { 
            get => _SelectedUser; 
            set
            {
                if (Set(ref _SelectedUser, value)) return;
                //if(_SelectedUser != null)
                _SelectedUser?.SettingFromRole(repo);
                _SelectedUser = value;
                repo.Save();
            }
        }

        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Добавить 
        //--------------------------------------------------------------------------------
        public ICommand AddCommand => new LambdaCommand(OnAddCommandExecuted, CanAddCommand);
        private bool CanAddCommand(object p) => true;
        private void OnAddCommandExecuted(object p)
        {
            RoleUserWrap newUser = new RoleUserWrap(new User { u_login = "Пользователь", u_role = 0 });
            //User newUser = new User { u_login = "Пользователь", u_role = 0 };

            ListUser.Add(newUser);
            //ListUserView.MoveCurrentToLast();
            SelectedUser = newUser;
            repo.Add(newUser.User, true);
        }
        //--------------------------------------------------------------------------------
        // Команда Удалить
        //--------------------------------------------------------------------------------
        public ICommand DeleteCommand => new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommand);
        private bool CanDeleteCommand(object p) => SelectedUser != null;
        private void OnDeleteCommandExecuted(object p)
        {
            if (MessageBox.Show($"Удалить {SelectedUser.User.u_login}", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (repo.Delete(SelectedUser.User, true))
                    ListUser.Remove(SelectedUser);
            }
        }

        //--------------------------------------------------------------------------------
        // Событие окончания редактирования ячейки
        //-------------------------------------------------------------------------------
        //public ICommand SelectOtdelCommand => new LambdaCommand(OnSelectOtdelCommandExecuted, CanSelectOtdelCommand);
        //private bool CanSelectOtdelCommand(object p) => true;
        //private void OnSelectOtdelCommandExecuted(object p)
        //{
        //    repoUser.Update(SelectedUser.User);
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
