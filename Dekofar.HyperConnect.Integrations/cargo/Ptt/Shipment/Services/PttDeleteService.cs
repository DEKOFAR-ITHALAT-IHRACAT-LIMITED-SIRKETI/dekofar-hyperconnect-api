using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Auth;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Responses;
using System.Text;
using System.Xml.Linq;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Services
{
    public class PttDeleteService : IPttDeleteService
    {
        private readonly IPttAuthService _authService;
        private readonly HttpClient _httpClient;
        private const string Endpoint = "https://pttws.ptt.gov.tr/PttVeriYuklemeTest/services/Sorgu"; // Test URL

        public PttDeleteService(IPttAuthService authService, HttpClient httpClient)
        {
            _authService = authService;
            _httpClient = httpClient;
        }

        public async Task<PttDeleteResponse> DeleteByReferenceAsync(string dosyaAdi, string referansNo)
        {
            var creds = _authService.GetCredentials();

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace kab = "http://kabul.ptt.gov.tr";
            XNamespace xsd = "http://kabul.ptt.gov.tr/xsd";

            var envelope = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "kab", kab),
                new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                new XElement(soapenv + "Body",
                    new XElement(kab + "referansVeriSil",
                        new XElement(kab + "inpRefDelete",
                            new XElement(xsd + "dosyaAdi", dosyaAdi),
                            new XElement(xsd + "musteriId", creds.CustomerId),
                            new XElement(xsd + "referansNo", referansNo),
                            new XElement(xsd + "sifre", creds.Password)
                        )
                    )
                )
            );

            return await PostAndParseAsync(envelope);
        }

        public async Task<PttDeleteResponse> DeleteByBarcodeAsync(string dosyaAdi, string barcode)
        {
            var creds = _authService.GetCredentials();

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace kab = "http://kabul.ptt.gov.tr";
            XNamespace xsd = "http://kabul.ptt.gov.tr/xsd";

            var envelope = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "kab", kab),
                new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                new XElement(soapenv + "Body",
                    new XElement(kab + "barkodVeriSil",
                        new XElement(kab + "inpDelete",
                            new XElement(xsd + "barcode", barcode),
                            new XElement(xsd + "dosyaAdi", dosyaAdi),
                            new XElement(xsd + "musteriId", creds.CustomerId),
                            new XElement(xsd + "sifre", creds.Password)
                        )
                    )
                )
            );

            return await PostAndParseAsync(envelope);
        }

        private async Task<PttDeleteResponse> PostAndParseAsync(XElement envelope)
        {
            var content = new StringContent(envelope.ToString(), Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(Endpoint, content);
            var xml = await response.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://kabul.ptt.gov.tr/xsd";

            return new PttDeleteResponse
            {
                HataKodu = int.TryParse(doc.Descendants(ns + "hataKodu").FirstOrDefault()?.Value, out var hk) ? hk : -1,
                Aciklama = doc.Descendants(ns + "aciklama").FirstOrDefault()?.Value ?? ""
            };
        }
    }
}
