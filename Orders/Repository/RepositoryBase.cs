using Orders.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Orders.Repository
{
    internal class RepositoryBase
    {
        protected ModelOrder db;

        public IQueryable<Route> Routes => db.Routes;
        public IQueryable<RouteStep> RouteSteps => db.RouteSteps;
        public IQueryable<User> Users => db.Users;
        public IQueryable<RouteType> RouteTypes => db.RouteTypes;
        public IQueryable<RouteOrder> RouteOrders => db.RouteOrders;//.Where(it => it.ro_parentId == null);
        //public IQueryable<Order> Orders => db.Orders.Include(it => it.RouteOrders.Select(it2 => it2.RouteAddings)   );
        public IQueryable<Order> Orders => db.Orders;
        public IQueryable<RouteStatus> RouteStatus => db.RouteStatus;
        public IQueryable<Role> Roles => db.Roles.AsNoTracking();



        //--------------------------------------------------------------------------------------------------
        // обновление записей в списке и вложенном списке
        //--------------------------------------------------------------------------------------------------
        public void Refresh() 
        {

            db.Dispose();
            SetConnect();

//#if !DEBUG
//            ConnectString = ConfigurationManager.ConnectionStrings["ModelLocal"].ConnectionString;
//#else
//            ConnectString = ConfigurationManager.ConnectionStrings["ModelOrder"].ConnectionString;
//            ConnectString += ";user id=fpLoginName;password=ctcnhjt,s";
//#endif
//            db = new ModelOrder(ConfigurationManager.ConnectionStrings["ModelLocal"].ConnectionString);

            //return;

            //if (item is null || item.Count == 0)
            //    return;

            //try
            //{

            //    foreach (var it in item)
            //        db.Entry(it).Reload();
            //}
            //catch { }
        }


        public void LoadRouteOrders(Order order)
        {
            db.Entry(order)
                .Collection(o => o.RouteOrders)
                .Query()
                .Where(it => it.ro_parentId == null)
                .OrderBy(o => o.ro_step)
                .Load();
        }

        //--------------------------------------------------------------------------------------------------
        // получение очередного номера
        //--------------------------------------------------------------------------------------------------
        public int GetNumberOrder()
        {
            int num = db.Database.SqlQuery<int>("select next value for numberOrder").FirstOrDefault();
            return num;
        }

        //--------------------------------------------------------------------------------------------------
        // сброс нумерации заказов
        //--------------------------------------------------------------------------------------------------
        public void ResetNumberOrder(int newNumber = 1)
        {
            db.Database.ExecuteSqlCommand($"alter sequence numberOrder restart with {newNumber}");
        }


        public IEnumerable<RouteOrder> GetRouteOrders(int OrderId)
        {
            IEnumerable<RouteOrder> listRoutes = db.RouteOrders.Where(it => it.ro_orderId == OrderId && it.ro_parentId == null)
                .OrderBy(o => o.ro_step);

            foreach(var item in listRoutes)
            {
                item.ChildRoutes = new ObservableCollection<RouteOrder> (
                    db.RouteOrders.Where(it => it.ro_parentId == item.id).OrderBy(o => o.ro_step));
            }

            return listRoutes;
        }


        private void SetConnect()
        {
            string ConnectString;

#if DEBUG
            ConnectString = ConfigurationManager.ConnectionStrings["ModelLocal"].ConnectionString;
#else
            ConnectString = ConfigurationManager.ConnectionStrings["ModelOrder"].ConnectionString;
            ConnectString += ";user id=fpLoginName;password=ctcnhjt,s";
#endif
            db = new ModelOrder(ConnectString);

        }


        //--------------------------------------------------------------------------------------------------
        // конструктор
        //--------------------------------------------------------------------------------------------------
        public RepositoryBase()
        {
            //            string ConnectString;

            //#if !DEBUG
            //            ConnectString = ConfigurationManager.ConnectionStrings["ModelLocal"].ConnectionString;
            //#else
            //            ConnectString = ConfigurationManager.ConnectionStrings["ModelOrder"].ConnectionString;
            //            ConnectString += ";user id=fpLoginName;password=ctcnhjt,s";
            //#endif

            //            db = new ModelOrder(ConnectString);

            SetConnect();

            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = true;

            Users.Load();
            RouteStatus.Load();
            RouteTypes.Load();
        }


        //public void LoadAddFiles(ICollection<RouteOrder> collection)
        //{
        //    foreach(var item in collection)
        //    {
        //        db.Entry(item).Collection(x => x.RouteAddings).Load();
        //    }
        //}

        //public void LoadUser(RouteOrder ro)
        //{
        //    db.Entry(ro).Reference(x => x.User).Load();
        //}

        //-----------------------------------------------------------------------------------------
        // сохранение базы
        //-----------------------------------------------------------------------------------------
        public void Save()
        {
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ShowMessage(e.Message);
                }
        }

        //-----------------------------------------------------------------------------------------
        // добавление записи
        //-----------------------------------------------------------------------------------------
        public bool Add<T>(T item, bool Autosave = false) where T : class, IEntity
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

                try
                {
                    //db.Entry(item).State = EntityState.Added;
                    db.Set<T>().Add(item);
                    if (Autosave)
                        db.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    ShowMessage(e.Message);
                    return false;
                }
        }


        //-----------------------------------------------------------------------------------------
        // удаление записи
        //-----------------------------------------------------------------------------------------
        public bool Delete<T>(T item, bool Autosave = false) where T : class, IEntity
        {

            if (item is null || item.id <= 0)
                return false;

                try
                {
                    db.Set<T>().Remove(item);
                    if (Autosave)
                        db.SaveChanges();

                    return true;
                }
                catch (Exception e)
                {
                    db.Entry(item).State = EntityState.Unchanged;
                    //ShowMessage(e.Message);
                    return false;
                }
        }


        //-----------------------------------------------------------------------------------------
        // удаление записи
        //-----------------------------------------------------------------------------------------
        public bool Update<T>(T item, bool Autosave = false) where T : class, IEntity
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

                try
                {
                    db.Entry(item).State = EntityState.Modified;
                    if (Autosave)
                        db.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    ShowMessage(e.Message);
                    return false;
                }
        }


        //-----------------------------------------------------------------------------------------
        // удаление записи
        //-----------------------------------------------------------------------------------------
        public bool DeleteRouteStep(RouteStep item, bool Autosave = false) 
        {

            if (item is null || item.id <= 0)
                return false;


                try
                {
                    db.RouteSteps.Remove(item);
                    if (Autosave)
                        db.SaveChanges();

                    return true;
                }
                catch (Exception e)
                {
                    ShowMessage(e.Message);
                    return false;
                }
        }



        void ShowMessage(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
