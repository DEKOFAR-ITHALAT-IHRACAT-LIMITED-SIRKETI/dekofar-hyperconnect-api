using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Services.sms
{
    public class NetGsmSmsSendService : INetGsmSmsSendService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetGsmSmsSendService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _defaultHeader;

        public NetGsmSmsSendService(
            IConfiguration configuration,
            ILogger<NetGsmSmsSendService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;

            var user = _configuration["NetGsm:Username"];
            var pass = _configuration["NetGsm:Password"];
            _defaultHeader = _configuration["NetGsm:DefaultHeader"] ?? "DEKOFAR LTD";

            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{user}:{pass}")
            );

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", auth);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<SmsSendResponse> SendAsync(SmsSendRequest request)
        {
            if (request.Messages == null || !request.Messages.Any())
            {
                return new SmsSendResponse
                {
                    Success = false,
                    Code = "CLIENT_ERR",
                    Description = "Messages boş olamaz"
                };
            }

            var url = _configuration["NetGsm:SendSmsBaseUrl"];

            // 🔑 Otomatik encoding (Türkçe karakter varsa UNICODE)
            var encoding = request.Messages.Any(m =>
                m.Msg.Any(c => "çğıİöşüÇĞÖŞÜ".Contains(c)))
                    ? "UNICODE"
                    : "DEFAULT";

            // ✅ Header garanti altına alındı
            var msgHeader = string.IsNullOrWhiteSpace(request.MsgHeader)
                ? _defaultHeader
                : request.MsgHeader;

            // 🔴 NETGSM V2 REST – DOĞRU BODY
            var body = new
            {
                msgheader = msgHeader,
                encoding = encoding,
                messages = request.Messages.Select(m => new
                {
                    no = m.No,
                    msg = m.Msg
                }).ToArray()
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("📤 NetGSM SMS Request: {Json}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var raw = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 NetGSM Raw Response: {Raw}", raw);

            // 🔴 NETGSM JSON RESPONSE
            if (raw.TrimStart().StartsWith("{"))
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                var code = root.GetProperty("code").GetString();

                return new SmsSendResponse
                {
                    RawResponse = raw,
                    Code = code ?? "ERR",
                    Success = code == "00",
                    JobId = root.TryGetProperty("jobid", out var job)
                        ? job.GetString()
                        : null,
                    Description = root.TryGetProperty("description", out var desc)
                        ? desc.GetString()
                        : null
                };
            }

            // 🔁 TEXT RESPONSE (fallback)
            if (raw.StartsWith("00"))
            {
                var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return new SmsSendResponse
                {
                    Success = true,
                    Code = "00",
                    JobId = parts.Length > 1 ? parts[1] : null,
                    RawResponse = raw,
                    Description = "Başarılı"
                };
            }

            return new SmsSendResponse
            {
                Success = false,
                Code = "NETGSM_ERR",
                RawResponse = raw,
                Description = "NetGSM bilinmeyen hata"
            };
        }
    }
}
