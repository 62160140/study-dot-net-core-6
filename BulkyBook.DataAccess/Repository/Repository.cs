using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            //บอก entity frame work ว่า Product มี fk ดังนี้
            //include แปลว่าประกอบด้วย
            //_db.Products.Include(u => u.Category).Include(u => u.CoverType);
            this.dbSet = _db.Set<T>();
        }

        void IRepository<T>.Add(T entity)
        {
            dbSet.Add(entity);
        }
        //includeProperties เช่น "Category,CoverType"
        IEnumerable<T> IRepository<T>.GetAll(Expression<Func<T, bool>>? filter = null,string ? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query= query.Where(filter);
            }
            if(includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)){
                    //บอก entity frame work ว่า Product มี fk ดังนี้
                    //include แปลว่าประกอบด้วย
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        T IRepository<T>.GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null,bool tracked = true)
        {
            IQueryable<T> query ;

            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //บอก entity frame work ว่า Product มี fk ดังนี้
                    //include แปลว่าประกอบด้วย
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        void IRepository<T>.Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        void IRepository<T>.RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
