using Orders.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Repository
{
    internal class RepositoryMain<T> : IRepository<T> where T : class, IEntity, new()
    {
        public static readonly  ModelOrder db = new ModelOrder();
        protected readonly DbSet<T> _Set;
        public virtual IQueryable<T> Items => _Set;

        public RepositoryMain()
        {
            _Set = db.Set<T>();
        }

        public bool Add(T item, bool Autosave = false)
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

        public bool Delete(int id, bool Autosave = false)
        {
            if (id < 1)
                return false;

            var item = db.Set<T>().FirstOrDefault(i => i.id == id) ?? new T { id = id };
            return Delete(item, Autosave);
        }

        public bool Delete(T item, bool Autosave = false)
        {

            if (item is null || item.id <= 0)
                return false;

            try
            {
                //db.Entry(item).State = EntityState.Deleted;
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

        public T Get(int id)
        {
            return Items.SingleOrDefault(it => it.id == id);
        }

        public void Save()
        {
            try
            {
                db.SaveChanges();
            }
            catch { }
        }

        public bool Update(T item, bool Autosave = false)
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
    }
}
