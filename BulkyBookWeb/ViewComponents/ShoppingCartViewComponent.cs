using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    //Component ต้อง exend ViewComponent
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //user loggedin
            if (claim != null)
            {   
                //have cookies
                if (HttpContext.Session.GetInt32(SD.SessionCartKey) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionCartKey));
                }
                //havn't cookies
                else
                {
                    //set cookies base on database
                    HttpContext.Session.SetInt32(SD.SessionCartKey, _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(SD.SessionCartKey));
                }
            }
            //user logout
            else
            {
                //clearSession
                HttpContext.Session.Clear();
                return View(0);
            }
        }

    }
}
