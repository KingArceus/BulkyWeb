using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;
        public Category Category { get; set; }

        public DeleteModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnGet(int categoryId)
        {
            if (categoryId != null && categoryId != 0)
            {
                Category = _dbContext.Categories.FirstOrDefault(Category => Category.Id == categoryId);
            }
        }

        public IActionResult OnPost(Category category)
        {
            var categoty = _dbContext.Categories.FirstOrDefault(c => c.Id == category.Id);
            if (categoty == null)
                return NotFound();

            _dbContext.Categories.Remove(categoty);
            _dbContext.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToPage("Index");
        }
    }
}
