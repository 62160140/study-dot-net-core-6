using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {


        private IUnitOfWork _unitOfWork;
        //ดึง Depency Injection มาใช้

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //GET
        public IActionResult Index()
        {

            return View();
        }




        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Company obj)
        {

            //ถ้า validation ผ่าน not null  ทั้งหมด และ  ต้องไม่มีกรณีที่มี ModelError จากบรรทัด 50
            if (ModelState.IsValid)
            {
                //Add data obj to database
                _unitOfWork.Companies.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Company created Successfullly";

                //RedirectTo Index
                return RedirectToAction("Index");
            }
            //หากValidation ไม่ผ่านส่งหน้าจอเดิมกลับและส่ง obj ตัวนี้ไปเพื่อไปแสดงใน tag span ใน tag-helper 
            return View(obj);

        }

        //GET
        public IActionResult Upsert(int? id)
        {

            //int? id = คือ param https://localhost:44379/Admin/Product/Upsert?id=1 

            //สร้าง ViewModel แล้วเดะส่งเป็น Object ไปทีเดียวเลย
            Company company = new();

            //สร้าง SelectListItem จากคำสั่ง . Select()

            //สร้าง SelectListItem จากคำสั่ง . Select()

            if (id == null || id == 0)
            {
                //create Product

                //ส่งด้วย ViewBag (ปลายทางไม่ต้อง cast Type)
                //ViewBag.CategoryList = CategoryList;
                //ส่งด้วย ViewData (ปลายทางต้อง cast Type)
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(company);
            }
            else
            {
                //update Product
                company = _unitOfWork.Companies.GetFirstOrDefault(i => i.Id == id);
            }



            return View(company);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj) //IFormFile => UploadFile
        {

            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    //Create
                    _unitOfWork.Companies.Add(obj);
                    TempData["success"] = "Company created Successfullly";

                }
                else
                {
                    //Update
                    _unitOfWork.Companies.Update(obj);

                    TempData["success"] = "Company updated Successfullly";
                }

                _unitOfWork.Save();
                //เก็บเข้า TempData

                //RedirectTo Index
                return RedirectToAction("Index");
            }
            //หากValidation ไม่ผ่านส่งหน้าจอเดิมกลับและส่ง obj ตัวนี้ไปเพื่อไปแสดงใน tag span ใน tag-helper 
            return View(obj);

        }





        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Companies.GetAll();
            return Json(new { data = companyList });
        }

        //POST
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
            var companyFromDbFirst = _unitOfWork.Companies.GetFirstOrDefault(c => c.Id == id);
            if (companyFromDbFirst == null)
            {
                return Json(new { success = false, message = "Error while deleteing" });
            }



            //Remove data obj to database
            _unitOfWork.Companies.Remove(companyFromDbFirst);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion

    }
}
