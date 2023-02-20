using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //ต้องเป็น admin เท่านั้นจะเข้าถึง path /customer/cart/... ได้
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {

        private IUnitOfWork _unitOfWork;


        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //GET
        public IActionResult Index()
        {
            //strongly type 
            IEnumerable<CoverType> objCategoryList = _unitOfWork.CoverType.GetAll();


            //pass data obj to View
            return View(objCategoryList);
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
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //Single จะ throw exception ถ้าหาไม่เจอ
            //SingleOrDefault จะไม่ throw exception ถ้าหาไม่เจอ แต่ถ้ามีหลายตัวจะ throw excep
            //FirstOrDefault ถ้าหาเจอตัววแรกจะเอาตัวแรก แต่ถ้าไม่มีไม่ throw excep
            //Find => หาprimary key ของ table

            //var categoryFromDb = _db.Categories.Find(id);
            var CoverTypeFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(c => c.Id == id);

            if (CoverTypeFromDbFirst == null)
            {
                return NotFound();
            }

            return View(CoverTypeFromDbFirst);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {

            //ถ้า validation ผ่าน not null  ทั้งหมด และ  ต้องไม่มีกรณีที่มี ModelError จากบรรทัด 50
            if (ModelState.IsValid)
            {
                //Add data obj to database
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                //เก็บเข้า TempData
                TempData["success"] = "CoverType updated Successfullly";

                //RedirectTo Index
                return RedirectToAction("Index");
            }
            //หากValidation ไม่ผ่านส่งหน้าจอเดิมกลับและส่ง obj ตัวนี้ไปเพื่อไปแสดงใน tag span ใน tag-helper 
            return View(obj);

        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //Single จะ throw exception ถ้าหาไม่เจอ
            //SingleOrDefault จะไม่ throw exception ถ้าหาไม่เจอ แต่ถ้ามีหลายตัวจะ throw excep
            //FirstOrDefault ถ้าหาเจอตัววแรกจะเอาตัวแรก แต่ถ้าไม่มีไม่ throw excep
            //Find => หาprimary key ของ table

            //var categoryFromDb = _db..Find(id);
            var categoryFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(c => c.Id == id);

            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            return View(categoryFromDbFirst);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {

            var categoryFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            //Remove data obj to database
            _unitOfWork.CoverType.Remove(categoryFromDbFirst);
            _unitOfWork.Save();
            TempData["success"] = "CoverType deleted Successfullly";

            //RedirectTo Index
            return RedirectToAction("Index");
        }
    }
}
