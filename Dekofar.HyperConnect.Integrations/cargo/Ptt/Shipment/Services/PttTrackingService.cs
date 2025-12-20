using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Auth;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Models.Responses;
using System.Text;
using System.Xml.Linq;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Services
{
    public class PttTrackingService : IPttTrackingService
    {
        private readonly IPttAuthService _authService;
        private readonly HttpClient _httpClient;
        private const string Endpoint = "https://pttws.ptt.gov.tr/GonderiTakipV2/services/Sorgu";

        public PttTrackingService(IPttAuthService authService, HttpClient httpClient)
        {
            _authService = authService;
            _httpClient = httpClient;
        }

        public async Task<PttTrackingResponse> GetByBarcodeAsync(string barcode)
        {
            var creds = _authService.GetCredentials();
            XNamespace xsd = "http://kargo.ptt.gov.tr/xsd";
            return await SendRequestAsync("barkodlaSorgula", new XElement(xsd + "barkod", barcode), creds);
        }

        public async Task<PttTrackingResponse> GetByReferenceAsync(string referenceNo)
        {
            var creds = _authService.GetCredentials();
            XNamespace xsd = "http://kargo.ptt.gov.tr/xsd";
            return await SendRequestAsync("referansNoIleSorgula", new XElement(xsd + "referansNo", referenceNo), creds);
        }

        private async Task<PttTrackingResponse> SendRequestAsync(string methodName, XElement searchElement, PttAuthCredentials creds)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace kab = "http://kargo.ptt.gov.tr";
            XNamespace xsd = "http://kargo.ptt.gov.tr/xsd";

            var envelope = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "kab", kab),
                new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                new XElement(soapenv + "Body",
                    new XElement(kab + methodName,
                        new XElement(kab + "input",
                            searchElement,
                            new XElement(xsd + "musteriId", creds.CustomerId),
                            new XElement(xsd + "kullanici", creds.Username),
                            new XElement(xsd + "sifre", creds.Password)
                        )
                    )
                )
            );

            var content = new StringContent(envelope.ToString(), Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(Endpoint, content);
            var xml = await response.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://kargo.ptt.gov.tr/xsd";

            var hataKodu = int.TryParse(doc.Descendants(ns + "hataKodu").FirstOrDefault()?.Value, out var hk) ? hk : -1;
            var aciklama = doc.Descendants(ns + "aciklama").FirstOrDefault()?.Value ?? "";

            var items = doc.Descendants(ns + "dongu").Select(d => new PttTrackingItem
            {
                Barkod = d.Element(ns + "barkod")?.Value ?? "",
                Durum = d.Element(ns + "durum")?.Value ?? "",
                IslemAdi = d.Element(ns + "islemAdi")?.Value ?? "",
                IslemYeri = d.Element(ns + "islemYeri")?.Value ?? "",
                IslemTarihi = DateTime.TryParse(d.Element(ns + "islemTarihi")?.Value, out var dt) ? dt : null
            }).ToList();

            return new PttTrackingResponse
            {
                HataKodu = hataKodu,
                Aciklama = aciklama,
                Items = items
            };
        }
    }
}
