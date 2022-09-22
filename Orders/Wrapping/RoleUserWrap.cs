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
            //ListRole = repo.Roles.ToList();
            User = user;
            SettingRole();
        }


        private void SettingRole()
        {
            foreach(var item in ListRole)
            {
                var r = User.RolesUser.FirstOrDefault(it => it.ru_role_id == item.role_Id);
                item.IsCheckRole = r is null ? false : true;
            }
        }

        private void SettingFromRole()
        {

        }

    }
}
