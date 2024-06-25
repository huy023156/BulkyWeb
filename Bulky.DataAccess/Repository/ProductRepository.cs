using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            Product? productFromDb = _db.Products.FirstOrDefault(u => u.Id == product.Id);

            if (productFromDb != null)
            {
                productFromDb.Id = product.Id;
				productFromDb.ISBN = product.ISBN;
				productFromDb.Price = product.Price;
				productFromDb.Description = product.Description;
				productFromDb.Price50 = product.Price50;
				productFromDb.Price100= product.Price100;
				productFromDb.CategoryId = product.CategoryId;
				productFromDb.Author = product.Author;
                productFromDb.ListPrice = product.ListPrice;
                if (product.ImageUrl != null)
                {
                    productFromDb.ImageUrl = product.ImageUrl;
                }

                _db.Products.Update(productFromDb);
            }

        }
    }
}
