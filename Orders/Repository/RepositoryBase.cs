using Orders.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Orders.Repository
{
    internal class RepositoryBase
    {
        protected ModelOrder db = new ModelOrder();

        public IQueryable<Route> Routes => db.Routes;
        public IQueryable<RouteStep> RouteSteps => db.RouteSteps;
        public IQueryable<User> Users => db.Users;
        public IQueryable<RouteType> RouteTypes => db.RouteTypes;
        public IQueryable<RouteOrder> RouteOrders => db.RouteOrders;
        public IQueryable<Order> Orders => db.Orders.Include(it => it.RouteOrders.Select(it2 => it2.RouteAddings)   );
        public IQueryable<RouteStatus> RouteStatus => db.RouteStatus;

               
        
        public void Refresh<T>(ICollection<T> item) 
        {
            if (item is null || item.Count == 0)
                return;

            //db.Entry(item).State = EntityState.Detached;

            try
            {

                foreach (var it in item)
                    db.Entry(it).Reload();
            }
            catch { }
        }


        public void GetAll()
        {
            List<Order> list = db.Orders.Where(it => it.o_stepRoute < 20).Include(ink => ink.RouteOrders).ToList();
            db.Entry(list[1]).Reload();
        }


        public RepositoryBase()
        {

            //db.Configuration.ProxyCreationEnabled = false;
            //db.Configuration.LazyLoadingEnabled = false;

                RouteTypes.Load();
                Users.Load();
                RouteStatus.Load();
        }

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
