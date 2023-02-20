using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInit
{
    public class Dbinit : IDbinit
    {
        private readonly UserManager<IdentityUser> _userManager;
        //เพิ่ม Role
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public Dbinit(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Init()
        {
            try
            {
                //Migration ถ้ายังไม่ได้ทำ => ไม่ต้องไปใช้คำสั่ง add-migration ใน console
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    //Migrate
                    _db.Database.Migrate();
                }




            }
            catch (Exception ex)
            {

            }

            //สร้าง role ถ้ายังไม่มี admin role 
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                //เพิ่ม Role ใน DB
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();
 
            }
            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
            //ถ้ายังไม่มี admin user
            if (user == null)
            {
                //สร้าง admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Pakawat",
                    PhoneNumber = "0990097351",
                    StreetAddress = "test 123 ",
                    State = "TH",
                    PostalCode = "11130",
                    City = "Bangkok"
                }, "@Homerun01").GetAwaiter().GetResult();
                //ค้นหาอีกที
                user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
                //Add role Admin ให้ user คนนี้
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
