﻿using Orders.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Repository
{
    internal class RepositoryBase
    {
        protected readonly ModelOrder db = new ModelOrder();

        public IQueryable<Route> Routes => db.Routes;
        public IQueryable<RouteStep> RouteSteps => db.RouteSteps;
        public IQueryable<User> Users => db.Users;
        public IQueryable<RouteType> RouteTypes => db.RouteTypes;
        public IQueryable<RouteOrder> RouteOrders => db.RouteOrders;
        public IQueryable<Order> Orders => db.Orders;

        //-----------------------------------------------------------------------------------------
        // сохранение базы
        //-----------------------------------------------------------------------------------------
        public void Save()
        {
            try
            {
                db.SaveChanges();
            }
            catch { }
        }

        //-----------------------------------------------------------------------------------------
        // добавление записи
        //-----------------------------------------------------------------------------------------
        public bool Add<T>(T item, bool Autosave = false) where T : class, IEntity
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            try
            {
                db.Entry(item).State = EntityState.Added;
                if (Autosave)
                    db.SaveChanges();
                return true;
            }
            catch
            {
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
            catch
            {
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
            catch
            {
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
            catch
            {
                return false;
            }
        }





    }
}