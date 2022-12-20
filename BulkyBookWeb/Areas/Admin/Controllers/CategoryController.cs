using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {

        private IUnitOfWork _unitOfWork;


        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //GET
        public IActionResult Index()
        {
            //strongly type 
            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();


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
        public IActionResult Create(Category obj)
        {
            //Validation ไม่ผ่าน (Server side Validation)
            //Custom Validation
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                //ถ้า key เป็นชื่อเดียวกับ model ทำให้ validation ใต้ field นั้นได้เลย
                //Name=name
                ModelState.AddModelError("DisplayOrder", "The DisplayOrder cannot exatly match the Name.");
            }

            //ถ้า validation ผ่าน not null  ทั้งหมด และ  ต้องไม่มีกรณีที่มี ModelError จากบรรทัด 50
            if (ModelState.IsValid)
            {
                //Add data obj to database
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created Successfullly";

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
            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault( c => c.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(c => c.Id == id);

            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            return View(categoryFromDbFirst);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            //Validation ไม่ผ่าน (Server side Validation)
            //Custom Validation
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                //ถ้า key เป็นชื่อเดียวกับ model ทำให้ validation ใต้ field นั้นได้เลย
                //Name=name
                ModelState.AddModelError("DisplayOrder", "The DisplayOrder cannot exatly match the Name.");
            }

            //ถ้า validation ผ่าน not null  ทั้งหมด และ  ต้องไม่มีกรณีที่มี ModelError จากบรรทัด 50
            if (ModelState.IsValid)
            {
                //Add data obj to database
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                //เก็บเข้า TempData
                TempData["success"] = "Category updated Successfullly";

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
            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault((System.Linq.Expressions.Expression<Func<Category, bool>>)(c => c.Id == id));
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

            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault((System.Linq.Expressions.Expression<Func<Category, bool>>)(c => c.Id == id));
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            //Remove data obj to database
            _unitOfWork.Category.Remove(categoryFromDbFirst);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted Successfullly";

            //RedirectTo Index
            return RedirectToAction("Index");
        }
    }
}
