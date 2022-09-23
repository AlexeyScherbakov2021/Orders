using Orders.Models;
using Orders.Repository;
using Orders.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Wrapping
{
    internal class RoleUserWrap
    {
        public User User { get; set; }
        public List<Role> ListRole { get; set; }

        //-------------------------------------------------------------------------------------------------------
        // коструктор
        //-------------------------------------------------------------------------------------------------------
        public RoleUserWrap(User user)
        {
            RepositoryBase repo = MainWindowViewModel.repo;
            ListRole = new List<Role>();
            foreach(var item in repo.Roles)
            {
                Role role = new Role();
                role.role_Id = item.role_Id;
                role.role_name = item.role_name;
                ListRole.Add(role);
            }
            User = user;
            SettingRole();
        }



        //-------------------------------------------------------------------------------------------------------
        // установка ролей из базы
        //-------------------------------------------------------------------------------------------------------
        private void SettingRole()
        {
            foreach(var item in ListRole)
            {
                var r = User.RolesUser.FirstOrDefault(it => it.ru_role_id == item.role_Id);
                item.IsCheckRole = r is null ? false : true;
            }
        }


        //-------------------------------------------------------------------------------------------------------
        // установка ролей в базу
        //-------------------------------------------------------------------------------------------------------
        public void SettingFromRole(RepositoryBase repo)
        {
            foreach(var item in ListRole)
            {
                RoleUser role = User.RolesUser.FirstOrDefault(it => it.ru_role_id == item.role_Id);
                if (item.IsCheckRole)
                {
                    if (role is null)
                    {
                        role = new RoleUser();
                        role.ru_user_id = User.id;
                        role.ru_role_id = item.role_Id;
                        User.RolesUser.Add(role);
                    }
                }
                else
                {
                    if (role != null)
                    {
                        repo.Delete<RoleUser>(role);
                        User.RolesUser.Remove(role);
                    }
                }
            }
        }

    }
}
