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
        protected readonly  ModelOrder db;
        protected readonly DbSet<T> _Set;
        public virtual IQueryable<T> Items => _Set;


        public RepositoryMain()
        {
            db = new ModelOrder();
            _Set = db.Set<T>();

        }

        public T Add(T item, bool Autosave = false)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            db.Entry(item).State = EntityState.Added;
            if (Autosave)
                db.SaveChanges();
            return item;
        }

        public void Delete(int id, bool Autosave = false)
        {
            if (id < 1)
                return;

            var item = _Set.Local.FirstOrDefault(i => i.id == id) ?? new T { id = id };
            Delete(item, Autosave);
        }

        public void Delete(T item, bool Autosave = false)
        {

            if (item is null || item.id <= 0)
                return;

            db.Entry(item).State = EntityState.Deleted;
            if (Autosave)
                db.SaveChanges();


        }

        public T Get(int id)
        {
            return Items.SingleOrDefault(it => it.id == id);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Update(T item, bool Autosave = false)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            db.Entry(item).State = EntityState.Modified;
            if (Autosave)
                db.SaveChanges();

        }
    }
}
