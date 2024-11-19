using FuCommunityWebModels.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;



namespace FuCommunityWebServices.Services
{
    public class VnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient();
        }

        public string CreateRequestUrl(OrderInfo order, string bankCode = "VNPAYQR", string locale = "vn")
        {
            // Ghi log để kiểm tra BankCode trước khi tạo request URL
            _logger.LogInformation($"Creating VNPAY request with BankCode: {bankCode}");

            var vnp_Params = new SortedList<string, string>();
            vnp_Params.Add("vnp_Version", "2.1.0");
            vnp_Params.Add("vnp_Command", "pay");
            vnp_Params.Add("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
            vnp_Params.Add("vnp_Amount", (order.Amount * 100).ToString());
            vnp_Params.Add("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnp_Params.Add("vnp_CurrCode", "VND");
            vnp_Params.Add("vnp_IpAddr", GetIpAddress());
            vnp_Params.Add("vnp_Locale", locale);
            vnp_Params.Add("vnp_OrderInfo", $"Thanh toan don hang: {order.OrderId}");
            vnp_Params.Add("vnp_OrderType", "other");
            vnp_Params.Add("vnp_ReturnUrl", _configuration["VnPay:Returnurl"]);
            vnp_Params.Add("vnp_TxnRef", order.OrderId.ToString());

            // Đảm bảo BankCode được thêm vào và ghi log
            vnp_Params.Add("vnp_BankCode", bankCode);
            _logger.LogInformation($"BankCode added to VNPAY request: {bankCode}");

            StringBuilder data = new StringBuilder();
            foreach (var kv in vnp_Params)
            {
                data.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
            }
            string queryString = data.ToString().TrimEnd('&');
            string secureHash = HmacSHA512(_configuration["VnPay:HashSecret"], queryString);
            string paymentUrl = $"{_configuration["VnPay:Url"]}?{queryString}&vnp_SecureHash={secureHash}";

            // Ghi log để kiểm tra URL thanh toán cuối cùng
            _logger.LogInformation($"Generated payment URL: {paymentUrl}");


            return paymentUrl;
        }



        public bool ValidateSignature(SortedList<string, string> vnpayData, string inputHash, string secretKey)
        {
            // Remove secure hash type and secure hash
            vnpayData.Remove("vnp_SecureHashType");
            vnpayData.Remove("vnp_SecureHash");

            StringBuilder data = new StringBuilder();
            foreach (var kv in vnpayData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
                }
            }
            string dataToHash = data.ToString().TrimEnd('&');
            string myChecksum = HmacSHA512(secretKey, dataToHash);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task<string> QueryDR(string orderId, string payDate)
        {
            var vnp_Api = _configuration["VNPAY:Api"];
            var vnp_HashSecret = _configuration["VNPAY:HashSecret"];
            var vnp_TmnCode = _configuration["VNPAY:TmnCode"];

            string vnp_RequestId = DateTime.Now.Ticks.ToString();
            string vnp_Version = "2.1.0";
            string vnp_Command = "querydr";
            string vnp_TxnRef = orderId;
            string vnp_OrderInfo = $"Truy van giao dich: {orderId}";
            string vnp_TransactionDate = payDate;
            string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string vnp_IpAddr = GetIpAddress();

            string signData = $"{vnp_RequestId}|{vnp_Version}|{vnp_Command}|{vnp_TmnCode}|{vnp_TxnRef}|{vnp_TransactionDate}|{vnp_CreateDate}|{vnp_IpAddr}|{vnp_OrderInfo}";
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

            var rfData = new
            {
                vnp_RequestId = vnp_RequestId,
                vnp_Version = vnp_Version,
                vnp_Command = vnp_Command,
                vnp_TmnCode = vnp_TmnCode,
                vnp_TxnRef = vnp_TxnRef,
                vnp_OrderInfo = vnp_OrderInfo,
                vnp_TransactionDate = vnp_TransactionDate,
                vnp_CreateDate = vnp_CreateDate,
                vnp_IpAddr = vnp_IpAddr,
                vnp_SecureHash = vnp_SecureHash
            };

            string jsonData = JsonSerializer.Serialize(rfData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(vnp_Api, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error querying DR: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> Refund(string orderId, long amount, string refundCategory, string payDate, string user)
        {
            var vnp_Api = _configuration["VNPAY:Api"];
            var vnp_HashSecret = _configuration["VNPAY:HashSecret"];
            var vnp_TmnCode = _configuration["VNPAY:TmnCode"];

            string vnp_RequestId = DateTime.Now.Ticks.ToString();
            string vnp_Version = "2.1.0";
            string vnp_Command = "refund";
            string vnp_TransactionType = refundCategory;
            long vnp_Amount = amount * 100;
            string vnp_TxnRef = orderId;
            string vnp_OrderInfo = $"Hoan tien giao dich: {orderId}";
            string vnp_TransactionNo = ""; // Assuming it's not provided
            string vnp_TransactionDate = payDate;
            string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string vnp_CreateBy = user;
            string vnp_IpAddr = GetIpAddress();

            string signData = $"{vnp_RequestId}|{vnp_Version}|{vnp_Command}|{vnp_TmnCode}|{vnp_TransactionType}|{vnp_TxnRef}|{vnp_Amount}|{vnp_TransactionNo}|{vnp_TransactionDate}|{vnp_CreateBy}|{vnp_CreateDate}|{vnp_IpAddr}|{vnp_OrderInfo}";
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

            var rfData = new
            {
                vnp_RequestId = vnp_RequestId,
                vnp_Version = vnp_Version,
                vnp_Command = vnp_Command,
                vnp_TmnCode = vnp_TmnCode,
                vnp_TransactionType = vnp_TransactionType,
                vnp_TxnRef = vnp_TxnRef,
                vnp_Amount = vnp_Amount,
                vnp_OrderInfo = vnp_OrderInfo,
                vnp_TransactionNo = vnp_TransactionNo,
                vnp_TransactionDate = vnp_TransactionDate,
                vnp_CreateBy = vnp_CreateBy,
                vnp_CreateDate = vnp_CreateDate,
                vnp_IpAddr = vnp_IpAddr,
                vnp_SecureHash = vnp_SecureHash
            };

            string jsonData = JsonSerializer.Serialize(rfData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(vnp_Api, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing refund: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private string HmacSHA512(string key, string data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                StringBuilder hash = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
                return hash.ToString();
            }
        }

        private string GetIpAddress()
        {
            var ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            return ip?.ToString() ?? "127.0.0.1";

        }
    }
}
