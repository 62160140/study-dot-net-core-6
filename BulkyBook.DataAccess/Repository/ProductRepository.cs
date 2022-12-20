using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {

        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(Product obj)
        {
            var productDB = _db.Products.FirstOrDefault(u=> u.Id == obj.Id);
            if (productDB != null)
            {
                productDB.Title= obj.Title;
                productDB.ISBN= obj.ISBN;
                productDB.Price= obj.Price;
                productDB.Price50= obj.Price50;
                productDB.ListPrice= obj.ListPrice;
                productDB.Price100= obj.Price100;
                productDB.Description= obj.Description;
                productDB.CategoryId= obj.CategoryId;
                productDB.Author= obj.Author;
                productDB.CoverTypeId= obj.CoverTypeId;
                if(obj.ImageUrl!=null)
                {
                    productDB.ImageUrl= obj.ImageUrl;
                }
            }
        }
    }
}
