using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Models.sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Services.sms
{
    public class NetGsmSmsSendService : INetGsmSmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetGsmSmsSendService> _logger;
        private readonly HttpClient _httpClient;

        public NetGsmSmsSendService(IConfiguration configuration, ILogger<NetGsmSmsSendService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public Task<List<SmsInboxResponse>> GetInboxMessagesAsync(SmsInboxRequest request)
            => throw new NotImplementedException("Inbox için ayrı servis kullanılmalı.");

        public async Task<SmsSendResponse> SendSmsAsync(SmsSendRequest request)
        {
            var baseUrl = _configuration["NetGsm:SendSmsBaseUrl"];
            var usercode = _configuration["NetGsm:Username"];
            var password = _configuration["NetGsm:Password"];

            // Kullanıcı kodu + şifreyi body’ye ekle
            var body = new
            {
                usercode,
                password,
                msgheader = request.MsgHeader,
                startdate = request.StartDate,
                stopdate = request.StopDate,
                appname = request.AppName,
                iysfilter = request.IysFilter,
                partnercode = request.PartnerCode,
                encoding = request.Encoding,
                messages = request.Messages
            };

            _logger.LogInformation("📤 NetGSM SMS JSON isteği gönderiliyor: {Url}", baseUrl);

            var response = await _httpClient.PostAsJsonAsync(baseUrl, body);
            var result = await response.Content.ReadFromJsonAsync<SmsSendResponse>();

            _logger.LogInformation("📥 NetGSM yanıtı: {Result}", result?.Description ?? "null");

            return result ?? new SmsSendResponse { Code = "ERR", Description = "Geçersiz yanıt" };
        }
    }
}
