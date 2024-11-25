using Bulky.DataAccess.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            string roleId = _dbContext.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _dbContext.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
                RoleList = _dbContext.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                }),
                CompanyList = _dbContext.Companies.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };

            roleManagementVM.ApplicationUser.Role = _dbContext.Roles.FirstOrDefault(u => u.Id == roleId).Name;
            return View(roleManagementVM);
        }

        [HttpPost]
        [ActionName("RoleManagement")]
        public IActionResult RoleManagementPOST(RoleManagementVM roleManagementVM)
        {
            string roleId = _dbContext.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _dbContext.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            if (roleManagementVM.ApplicationUser.Role != oldRole)
            {
                ApplicationUser applicationUser = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId; 

                if (oldRole == SD.Role_Company)
                    applicationUser.CompanyId = null;

                _dbContext.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult() ;
            }

            return RedirectToAction(nameof(Index));
        }

        #region APICALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _dbContext.ApplicationUsers.Include(u => u.Company).ToList();

            var userRoles = _dbContext.UserRoles.ToList();
            var roles = _dbContext.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

                if (user.Company == null)
                    user.Company = new() { Name = "" };
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var user = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return Json(new { success = false, message = "Error while Locking/Unlocking" });

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
                user.LockoutEnd = DateTime.Now;
            else
                user.LockoutEnd = DateTime.Now.AddDays(30);

            _dbContext.SaveChanges();

            return Json(new {success = true, message = "Operation Successful"});
        }

        #endregion
    }
}
