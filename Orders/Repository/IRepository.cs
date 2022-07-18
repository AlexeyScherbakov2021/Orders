using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Repository
{
    interface IRepository<T> where T : class, IEntity, new()
    {
        IQueryable<T> Items { get; }
        T Get(int id);
        bool Add(T item, bool Autosave = false);
        bool Delete(int id, bool Autosave = false);
        bool Delete(T item, bool Autosave = false);
        bool Update(T item, bool Autosave = false);
        void Save();

    }
}
