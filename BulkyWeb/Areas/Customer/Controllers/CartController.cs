using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }

		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()
			};

			foreach (var item in ShoppingCartVM.ShoppingCartsList)
			{
				item.Price = GetPriceBasedOnQuantity(item);

				ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);

				ViewData["OrderCount"] = item.Count;
			}

			return View(ShoppingCartVM);
		}

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


			foreach (var item in ShoppingCartVM.ShoppingCartsList)
			{
				item.Price = GetPriceBasedOnQuantity(item);

				ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);

				ViewData["OrderCount"] = item.Count;

			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM.ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

			ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


			foreach (var item in ShoppingCartVM.ShoppingCartsList)
			{
				item.Price = GetPriceBasedOnQuantity(item);

				ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);

				ViewData["OrderCount"] = item.Count;

			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//it is a regular customer acc and we need to capture the payment
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				//It is a company acc and we dont need to capture the amount, we have to capture the amout of after 30 days
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}

			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.ShoppingCartsList)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,
				};

				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}


			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//it is a regular customer
				//Stripe Logic

				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = "https://example.com/success",
					LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
					{
						new Stripe.Checkout.SessionLineItemOptions
						{
							Price = "price_1MotwRLkdIwHu7ixYcPLm5uZ",
							Quantity = 2,
						},
					},
					Mode = "payment",
				};
				var service = new Stripe.Checkout.SessionService();
				service.Create(options);

			}


			return RedirectToAction(nameof(OrderConfirmation), new { Id = ShoppingCartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int Id)
		{
			return View(Id);
		}

		public IActionResult UpdateAndRemoveProduct(int cartId, string operation)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			if (operation == "plus")
			{
				cartFromDb.Count += 1;
				_unitOfWork.ShoppingCart.Update(cartFromDb);
			}
			if (operation == "minus")
			{
				if (cartFromDb.Count <= 1)
				{
					_unitOfWork.ShoppingCart.Remove(cartFromDb);
				}
				else
				{
					cartFromDb.Count -= 1;
					_unitOfWork.ShoppingCart.Update(cartFromDb);
				}
			}
			if (operation == "remove")
			{
				_unitOfWork.ShoppingCart.Remove(cartFromDb);
			}

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}




		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Count <= 50)
			{
				return shoppingCart.Product.Price;
			}
			else
			{
				if (shoppingCart.Count <= 100)
				{
					return shoppingCart.Product.Price50;
				}
				else
				{
					return shoppingCart.Product.Price100;
				}
			}
		}
	}
}
