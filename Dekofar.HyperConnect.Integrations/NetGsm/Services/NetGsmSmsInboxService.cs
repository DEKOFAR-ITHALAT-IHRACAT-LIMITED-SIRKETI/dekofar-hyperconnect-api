using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Services.sms
{
    public class NetGsmSmsInboxService : INetGsmSmsInboxService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetGsmSmsInboxService> _logger;
        private readonly HttpClient _httpClient;

        public NetGsmSmsInboxService(
            IConfiguration configuration,
            ILogger<NetGsmSmsInboxService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<List<SmsInboxResponse>> GetInboxAsync(SmsInboxRequest request)
        {
            var user = _configuration["NetGsm:Username"];
            var pass = _configuration["NetGsm:Password"];
            var url = _configuration["NetGsm:ReceiveSmsBaseUrl"];

            var start = DateTime.ParseExact(
                request.StartDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture
            ).ToString("ddMMyyyyHHmm");

            var stop = DateTime.ParseExact(
                request.StopDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture
            ).ToString("ddMMyyyyHHmm");

            var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <mainbody>
              <header>
                <usercode>{user}</usercode>
                <password>{pass}</password>
                <startdate>{start}</startdate>
                <stopdate>{stop}</stopdate>
                <type>1</type>
              </header>
            </mainbody>
            """;

            var content = new StringContent(xml, Encoding.UTF8, "application/xml");
            var response = await _httpClient.PostAsync(url, content);
            var raw = await response.Content.ReadAsStringAsync();

            if (raw.TrimStart().StartsWith("<"))
            {
                var doc = XDocument.Parse(raw);
                return doc.Descendants("inbox")
                    .Select(x => new SmsInboxResponse
                    {
                        Orjinator = x.Element("gsmno")?.Value ?? "",
                        Message = x.Element("msg")?.Value ?? "",
                        Date = x.Element("date")?.Value ?? ""
                    }).ToList();
            }

            return raw.Split("<br>", StringSplitOptions.RemoveEmptyEntries)
                .Select(l =>
                {
                    var p = l.Split('|');
                    return new SmsInboxResponse
                    {
                        Orjinator = p.ElementAtOrDefault(0) ?? "",
                        Message = p.ElementAtOrDefault(1) ?? "",
                        Date = p.ElementAtOrDefault(2) ?? ""
                    };
                }).ToList();
        }
    }
}
