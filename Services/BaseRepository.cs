using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeManager.Services
{
    public abstract class BaseRepository<T> : IRepository<T>
    {
        protected List<T> _items = new List<T>();
        protected readonly string _filePath;
        protected readonly object _lock = new object();

        protected BaseRepository(string filePath)
        {
            _filePath = filePath;
            Load();
        }

        public virtual List<T> GetAll() => _items.ToList();

        public virtual T? GetById(int id)
        {
            return _items.FirstOrDefault(item => GetId(item) == id);
        }

        public virtual void Add(T entity)
        {
            lock (_lock)
            {
                SetId(entity, GenerateNewId());
                _items.Add(entity);
                Save();
            }
        }

        public virtual void Update(T entity)
        {
            lock (_lock)
            {
                var existing = GetById(GetId(entity));
                if (existing != null)
                {
                    var index = _items.IndexOf(existing);
                    _items[index] = entity;
                    Save();
                }
            }
        }

        public virtual void Delete(int id)
        {
            lock (_lock)
            {
                var entity = GetById(id);
                if (entity != null)
                {
                    _items.Remove(entity);
                    Save();
                }
            }
        }

        protected abstract int GetId(T entity);
        protected abstract void SetId(T entity, int id);
        protected abstract string Serialize(T entity);
        protected abstract T Deserialize(string line);
        public abstract void Save();
        public abstract void Load();

        protected int GenerateNewId()
        {
            return _items.Count > 0 ? _items.Max(item => GetId(item)) + 1 : 1;
        }
    }
}
