using Azure.Core;
using FuCommunityWebModels.Models;
using Microsoft.AspNetCore.Mvc;
using FuCommunityWebServices.Services;
using FuCommunityWebDataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FuCommunityWebDataAccess.Data;

namespace FUCommunityWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;
        private readonly VnPayService _vnPayService;
        private readonly OrderRepo _orderRepository;
        private readonly UserService _userService;

        public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger, VnPayService vnPayService, OrderRepo orderRepository, UserService userService)
        {
            _configuration = configuration;
            _logger = logger;
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Pay()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Pay(OrderInfo order, string paymentMethod, string locale, int Amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Received payment method: {paymentMethod}");

            if (string.IsNullOrEmpty(paymentMethod))
            {
                _logger.LogWarning("Payment method is null or empty.");
                ModelState.AddModelError("paymentMethod", "Please select payment method.");
                return View(order);
            }

            if (Amount <= 0)
            {
                _logger.LogWarning("Invalid amount entered.");
                ModelState.AddModelError("Amount", "Please enter a valid amount.");
                return View(order);
            }

            order.Amount = Amount;
            order.Status = "0";
            order.CreatedDate = DateTime.Now;
            order.UserID = userId;

            string bankCode = paymentMethod switch
            {
                "VNPAYQR" => "VNPAYQR",
                "VNBANK" => "VNBANK",
                "INTCARD" => "INTCARD",
                _ => throw new ArgumentException("Invalid payment method."),
            };

            order.BankCode = bankCode;

            await _orderRepository.AddOrderAsync(order);

            _logger.LogInformation($"Determined BankCode: {bankCode}");

            string selectedLocale = locale switch
            {
                "en" => "en",
                _ => "vn",
            };

            string paymentUrl = _vnPayService.CreateRequestUrl(order, bankCode, selectedLocale);
            _logger.LogInformation($"Generated VNPAY URL: {paymentUrl}");

            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult QueryDR()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QueryDR(string orderId, string payDate)
        {
            string response = await _vnPayService.QueryDR(orderId, payDate);
            ViewBag.Response = response;
            return View();
        }

        [HttpGet]
        public IActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Refund(string orderId, long amount, string refundCategory, string payDate, string user)
        {
            string response = await _vnPayService.Refund(orderId, amount, refundCategory, payDate, user);
            ViewBag.Response = response;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IPN()
        {
            var vnpayData = Request.Form;
            SortedList<string, string> sortedVnpayData = new SortedList<string, string>();
            foreach (var key in vnpayData.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    sortedVnpayData.Add(key, vnpayData[key]);
                }
            }

            string vnp_HashSecret = _configuration["VnPay:HashSecret"];
            string inputHash = vnpayData["vnp_SecureHash"];
            bool isValid = _vnPayService.ValidateSignature(sortedVnpayData, inputHash, vnp_HashSecret);

            string returnContent = string.Empty;

            if (isValid)
            {
                long orderId = Convert.ToInt64(sortedVnpayData["vnp_TxnRef"]);
                long amount = Convert.ToInt64(sortedVnpayData["vnp_Amount"]) / 100;
                long vnpayTranId = Convert.ToInt64(sortedVnpayData["vnp_TransactionNo"]);
                string responseCode = sortedVnpayData["vnp_ResponseCode"];
                string transactionStatus = sortedVnpayData["vnp_TransactionStatus"];

                OrderInfo order = await _orderRepository.GetOrderByIdAsync(orderId);

                if (order != null)
                {
                    if (order.Amount == amount)
                    {
                        if (order.Status == "0")
                        {
                            if (responseCode == "00" && transactionStatus == "00")
                            {
                                _logger.LogInformation($"Payment successful, OrderId={orderId}, VNPAY TranId={vnpayTranId}");
                                order.Status = "1";
                                ViewBag.Message = "Transaction successful";
                            }
                            else
                            {
                                _logger.LogInformation($"Payment failed, OrderId={orderId}, VNPAY TranId={vnpayTranId}, ResponseCode={responseCode}");
                                order.Status = "2";
                                ViewBag.Message = $"An error occurred. Error code: {responseCode}";
                            }

                            await _orderRepository.UpdateOrderAsync(order);

                            returnContent = "{\"RspCode\":\"00\",\"Message\":\"Confirm Success\"}";
                        }
                        else
                        {
                            returnContent = "{\"RspCode\":\"02\",\"Message\":\"Order already confirmed\"}";
                        }
                    }
                    else
                    {
                        returnContent = "{\"RspCode\":\"04\",\"Message\":\"Invalid amount\"}";
                    }
                }
                else
                {
                    returnContent = "{\"RspCode\":\"01\",\"Message\":\"Order not found\"}";
                }
            }
            else
            {
                _logger.LogInformation($"Invalid signature, InputData={Request.Form}");
                returnContent = "{\"RspCode\":\"97\",\"Message\":\"Invalid signature\"}";
            }

            return Content(returnContent, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> Return()
        {
            var vnpayData = Request.Query;
            SortedList<string, string> sortedVnpayData = new SortedList<string, string>();
            foreach (var key in vnpayData.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    sortedVnpayData.Add(key, vnpayData[key]);
                }
            }

            string vnp_HashSecret = _configuration["VnPay:HashSecret"];
            string inputHash = vnpayData["vnp_SecureHash"];
            bool isValid = _vnPayService.ValidateSignature(sortedVnpayData, inputHash, vnp_HashSecret);

            if (isValid)
            {
                string responseCode = sortedVnpayData["vnp_ResponseCode"];
                if (responseCode == "00")
                {
                    ViewBag.Message = "Transaction was successful. Thank you for using our service.";

                    var orderId = Convert.ToInt64(sortedVnpayData["vnp_TxnRef"]);
                    var amount = Convert.ToInt64(sortedVnpayData["vnp_Amount"]) / 100;

                    var order = await _orderRepository.GetOrderByIdAsync(orderId);

                    if (order != null && order.Status == "0")
                    {
                        order.Status = "1";
                        await _orderRepository.UpdateOrderAsync(order);

                        var userId = order.UserID;
                        var user = await _userService.GetUserByIdAsync(userId);

                        if (user != null)
                        {
                            user.Point += amount / 1000;

                            await _userService.UpdateUserAsync(user);

                            _logger.LogInformation($"User {user.UserName} has been awarded {amount} points.");
                        }
                    }
                }
                else if (responseCode == "24")
                {
                    return RedirectToAction("Pay", "VnPay");
                }
                else
                {
                    ViewBag.Message = $"An error occurred during processing. Error code: {responseCode}";
                }
            }
            else
            {
                ViewBag.Message = "An error occurred during processing.";
            }

            return View();
        }
    }
}