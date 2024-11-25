using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var companyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return View(companyList);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company = _unitOfWork.CompanyRepository.Get(u => u.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                    _unitOfWork.CompanyRepository.Add(company);
                else
                    _unitOfWork.CompanyRepository.Update(company);

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }

            return View(company);
        }

        #region APICALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var deleteProduct = _unitOfWork.CompanyRepository.Get(u => u.Id == id);
            if (deleteProduct == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.CompanyRepository.Remove(deleteProduct);
            _unitOfWork.Save();

            var productList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
