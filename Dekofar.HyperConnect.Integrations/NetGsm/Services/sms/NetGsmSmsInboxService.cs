using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Models.sms;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Services.sms
{
    public class NetGsmSmsInboxService : INetGsmSmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetGsmSmsInboxService> _logger;
        private readonly HttpClient _httpClient;

        public NetGsmSmsInboxService(IConfiguration configuration, ILogger<NetGsmSmsInboxService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<List<SmsInboxResponse>> GetInboxMessagesAsync(SmsInboxRequest request)
        {
            var usercode = _configuration["NetGsm:Username"];
            var password = _configuration["NetGsm:Password"];
            var baseUrl = _configuration["NetGsm:ReceiveSmsBaseUrl"];

            var start = DateTime.Parse(request.StartDate).ToString("ddMMyyyyHHmm");
            var stop = DateTime.Parse(request.StopDate).ToString("ddMMyyyyHHmm");

            var xmlBody = new StringBuilder();
            xmlBody.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xmlBody.AppendLine("<mainbody>");
            xmlBody.AppendLine("  <header>");
            xmlBody.AppendLine($"    <usercode>{usercode}</usercode>");
            xmlBody.AppendLine($"    <password>{password}</password>");
            xmlBody.AppendLine($"    <startdate>{start}</startdate>");
            xmlBody.AppendLine($"    <stopdate>{stop}</stopdate>");
            xmlBody.AppendLine("    <type>1</type>");
            xmlBody.AppendLine("  </header>");
            xmlBody.AppendLine("</mainbody>");

            var content = new StringContent(xmlBody.ToString(), Encoding.UTF8, "application/xml");

            var response = await _httpClient.PostAsync(baseUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"NetGSM isteği başarısız: {response.StatusCode}");

            if (!responseContent.TrimStart().StartsWith("<"))
            {
                return responseContent
                    .Split("<br>", StringSplitOptions.RemoveEmptyEntries)
                    .Select(line =>
                    {
                        var parts = line.Split('|');
                        return new SmsInboxResponse
                        {
                            Orjinator = parts.ElementAtOrDefault(0)?.Trim(),
                            Message = parts.ElementAtOrDefault(1)?.Trim(),
                            Date = parts.ElementAtOrDefault(2)?.Trim()
                        };
                    })
                    .ToList();
            }
            else
            {
                var xml = XDocument.Parse(responseContent);
                var inboxElements = xml.Descendants("inbox");

                return inboxElements.Select(x => new SmsInboxResponse
                {
                    Orjinator = x.Element("gsmno")?.Value,
                    Message = x.Element("msg")?.Value,
                    Date = x.Element("date")?.Value
                }).ToList();
            }
        }

        // Inbox servisinde gönderim kullanılmaz
        public Task<SmsSendResponse> SendSmsAsync(SmsSendRequest request)
            => throw new NotImplementedException("Bu servis sadece inbox içindir.");
    }
}
