using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //บอกว่าหน้านีี้มีการ Authorize เนื้อหา order ระหว่าง employee กับ user ต่างกัน
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM   OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        //Payment later
        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_Pay_Now()
        {
            //ORderVM Bindproperty มา
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //Stripe settings
            var domain = "https://localhost:7051/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>()
                ,
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };
            //วนproductที่ซื้อทั้งหมดเพื่อ add เข้าไปที่ stripe
            foreach (var item in OrderVM.OrderDetail)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);

            }
            //end วนproductที่ซื้อ

            //CreateSession
            var service = new SessionService();
            Session session = service.Create(options);

            //Save StripeId ลงไปที่ OrderHeader
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            //end Save OrderHeader

            Response.Headers.Add("Location", session.Url);
            //end stripe
            return new StatusCodeResult(303);
        }


        //Pay later
        public IActionResult PaymentConfirmation(int orderHeaderid)
        //id มาจาก params หรือตอน RedirectToAction
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
            // userId
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //GetSession
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus , SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            //pass Id(int) ของ OrderHeader ไปด้วย
            return View(orderHeaderid);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]

        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            //OrderVM.OrderHeader มาจากการ Bind Property
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u=> u.Id == OrderVM.OrderHeader.Id,tracked:false);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress= OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City= OrderVM.OrderHeader.City;
            orderHeaderFromDb.State= OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode= OrderVM.OrderHeader.PostalCode;
            if(OrderVM.OrderHeader.Carrier!= null)
            {
                orderHeaderFromDb.Carrier= OrderVM.OrderHeader.Carrier;
            }
            if(OrderVM.OrderHeader.TrackingNumber!= null)
            {
                orderHeaderFromDb.TrackingNumber= OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Update  Successfully!";
            return RedirectToAction("Details","Order",new {orderId = orderHeaderFromDb.Id});
        }


        [HttpPost]

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            //OrderVM.OrderHeader มาจากการ Bind Property
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully";
            return RedirectToAction("Details" , "Order", new {orderId = OrderVM.OrderHeader.Id});
            
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            //OrderVM.OrderHeader มาจากการ Bind Property
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == OrderVM.OrderHeader.Id,tracked:false);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction("Details" , "Order" , new {orderId = OrderVM.OrderHeader.Id});

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            //OrderVM.OrderHeader มาจากการ Bind Property
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully";

            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });

        }




        #region api
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeader;

            //Authorize 
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                //Admin
                orderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                //User => เห็นเฉพาะของตัวเอง
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeader = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            //Where => ตาม params
            switch (status)
            {
                case "pending":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }

            return Json(new { data = orderHeader });
        }
        #endregion
    }
}
