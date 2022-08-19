using Orders.Models;
using System;
using System.Collections.Generic;
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
        public IQueryable<RouteOrder> RouteOrders => db.RouteOrders;
        //public IQueryable<Order> Orders => db.Orders.Include(it => it.RouteOrders.Select(it2 => it2.RouteAddings)   );
        public IQueryable<Order> Orders => db.Orders;
        public IQueryable<RouteStatus> RouteStatus => db.RouteStatus;



        //--------------------------------------------------------------------------------------------------
        // обновление записей в списке и вложенном списке
        //--------------------------------------------------------------------------------------------------
        public void Refresh<T>(ICollection<T> item) 
        {

            db.Dispose();
            db = new ModelOrder(ConfigurationManager.ConnectionStrings["ModelLocal"].ConnectionString);

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


        //--------------------------------------------------------------------------------------------------
        // конструктор
        //--------------------------------------------------------------------------------------------------
        public RepositoryBase()
        {
            string ConnectString;

#if DEBUG
            ConnectString = ConfigurationManager.ConnectionStrings["ModelLocal"].ConnectionString;
#else
            ConnectString = ConfigurationManager.ConnectionStrings["ModelOrder"].ConnectionString;
            ConnectString += ";user id=fpLoginName;password=ctcnhjt,s";
#endif

            db = new ModelOrder(ConnectString);

            //db.Configuration.ProxyCreationEnabled = false;
            //db.Configuration.LazyLoadingEnabled = false;

            RouteTypes.Load();
            Users.Load();
            RouteStatus.Load();
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
                    ShowMessage(e.Message);
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
