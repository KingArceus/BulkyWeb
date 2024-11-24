using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public void Initialize()
        {
            try
            {
                if (_dbContext.Database.GetPendingMigrations().Count() > 0)
                    _dbContext.Database.Migrate();
            }
            catch (Exception ex) { }

            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "khangtpc@gmail.com",
                    Email = "khangtpc@gmail.com",
                    Name = "King Arceus",
                    PhoneNumber = "0965184529",
                    StreetAdress = "49/19 Me Tri",
                    State = "Nam Tu Liem",
                    PostalCode = "100000",
                    City = "Ha Noi"
                }, "K1n9@@ceu5").GetAwaiter().GetResult();

                ApplicationUser user = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == "khangtpc@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
