using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //ต้องเป็น admin เท่านั้นจะเข้าถึง path /customer/cart/... ได้
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {

        private IUnitOfWork _unitOfWork;
        //ดึง Depency Injection มาใช้
        private readonly IWebHostEnvironment _hostEnvirontment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvirontment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvirontment = hostEnvirontment;
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
        public IActionResult Create(CoverType obj)
        {

            //ถ้า validation ผ่าน not null  ทั้งหมด และ  ต้องไม่มีกรณีที่มี ModelError จากบรรทัด 50
            if (ModelState.IsValid)
            {
                //Add data obj to database
                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "CoverType created Successfullly";

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
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            //สร้าง SelectListItem จากคำสั่ง . Select()

            //สร้าง SelectListItem จากคำสั่ง . Select()

            if (id == null || id == 0)
            {
                //create Product

                //ส่งด้วย ViewBag (ปลายทางไม่ต้อง cast Type)
                //ViewBag.CategoryList = CategoryList;
                //ส่งด้วย ViewData (ปลายทางต้อง cast Type)
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                //update Product
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(i => i.Id == id);
            }

       

            return View(productVM);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj,IFormFile file) //IFormFile => UploadFile
        {

            if (ModelState.IsValid)
            {
                //ดึง Path ของ wwwroot มา
                string wwwRootPath = _hostEnvirontment.WebRootPath;
                if (file != null)
                {
                    //ถ้า file ถูก upload มา
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images/products");
                    var extension = Path.GetExtension(file.FileName);

                    //อัพเดทภาพ 
                    if(obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using(var fileStrems = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        //นำ file Copy ไปที่ fileStream
                        file.CopyTo(fileStrems);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (obj.Product.Id == 0)
                {
                    //Create
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    //Update
                    _unitOfWork.Product.Update(obj.Product);

                }

                _unitOfWork.Save();
                //เก็บเข้า TempData
                TempData["success"] = "Product updated Successfullly";

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
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new {data=productList});
        }

        //POST
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
            var categoryFromDbFirst = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
            if (categoryFromDbFirst == null)
            {
                return Json(new { success = false, message = "Error while deleteing" });
            }


            var oldImagePath = Path.Combine(_hostEnvirontment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            //Remove data obj to database
            _unitOfWork.Product.Remove(categoryFromDbFirst);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion
    }
}
