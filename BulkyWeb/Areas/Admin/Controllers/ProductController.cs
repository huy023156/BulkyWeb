using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(products);
        }

        public ActionResult Upsert(int? id) {
			ProductVM productVM = new ProductVM
			{
				CategoryList = _unitOfWork.Category
				.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				}),
				Product = new Product()
		};

			if (id == 0 || id == null)
			{
			} else
			{
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
			}

			return View(productVM);
        }

        [HttpPost]
        public ActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\product");
				
					if(!string.IsNullOrEmpty(productVM.Product.ImageUrl)) {
						//delete old img
						var oldImgPath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
						
						if (System.IO.File.Exists(oldImgPath))
						{
							System.IO.File.Delete(oldImgPath);
						}
					}
						
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}

					productVM.Product.ImageUrl = @"\images\product\" + fileName;
				}

				if (productVM.Product.Id == 0)
				{
					_unitOfWork.Product.Add(productVM.Product);
					TempData["success"] = "Product created successfully";
				}
				else
				{
					_unitOfWork.Product.Update(productVM.Product);
					TempData["success"] = "Product updated successfully";
				}

				_unitOfWork.Save();
				return RedirectToAction("Index");
			} else
			{
				productVM.CategoryList = _unitOfWork.Category
				.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				});
			}

			return View(productVM);
		}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll() 
		{
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = products });
        }

		[HttpDelete]
		public IActionResult Delete(int id)
		{
			Product productToDelete = _unitOfWork.Product.Get(u => u.Id == id);

			if (productToDelete == null) {
				return Json(new {success = false, Message= "Delete failed" });
			}

            if (!string.IsNullOrEmpty(productToDelete.ImageUrl))
            {
                //delete old img
                var oldImgPath = Path.Combine(_webHostEnvironment.WebRootPath, productToDelete.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImgPath))
                {
                    System.IO.File.Delete(oldImgPath);
                }
            }

			_unitOfWork.Product.Remove(productToDelete);
			_unitOfWork.Save();
			return Json(new { success = true, Message = "Delete successful" });
        }

        #endregion
    }
}
