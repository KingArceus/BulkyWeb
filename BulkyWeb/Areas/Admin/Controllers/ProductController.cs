using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            var productList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category").ToList();
            return View(productList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.ProductRepository.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? formFile)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (formFile != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldPath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                    {
                        formFile.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + filename;
                }

                if (productVM.Product.Id == 0)
                    _unitOfWork.ProductRepository.Add(productVM.Product);
                else
                    _unitOfWork.ProductRepository.Update(productVM.Product);
                
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }

            productVM.CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            return View(productVM);
        }
        
        #region APICALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new {data =  productList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var deleteProduct = _unitOfWork.ProductRepository.Get(u => u.Id == id);
            if (deleteProduct == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, deleteProduct.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);

            _unitOfWork.ProductRepository.Remove(deleteProduct);
            _unitOfWork.Save();

            var productList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new { success = true, message = "Dlete Successful" });
        }

        #endregion
    }
}
