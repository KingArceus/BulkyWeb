using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public CategoryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var categoryList = _dbContext.Categories.ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
                ModelState.AddModelError("name", "Category Name cannot match exactly to Display Order");

            if (ModelState.IsValid)
            {
                _dbContext.Categories.Add(category);
                _dbContext.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Edit(int? categoryId)
        {
            if (categoryId == null || categoryId == 0)
                return NotFound();

            var categoty = _dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (categoty == null)
                return NotFound();

            return View(categoty);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Categories.Update(category);
                _dbContext.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int? categoryId)
        {
            if (categoryId == null || categoryId == 0)
                return NotFound();
        
            var categoty = _dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (categoty == null)
                return NotFound();
        
            return View(categoty);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? categoryId)
        {
            var categoty = _dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (categoty == null)
                return NotFound();

            _dbContext.Categories.Remove(categoty);
            _dbContext.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
