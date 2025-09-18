using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Auth;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Requests;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Responses;
using System.Text;
using System.Xml.Linq;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Services
{
    public class PttShipmentService : IPttShipmentService
    {
        private readonly IPttAuthService _authService;
        private readonly HttpClient _httpClient;
        private const string Endpoint = "https://pttws.ptt.gov.tr/PttVeriYuklemeTest/services/Sorgu"; // Test

        public PttShipmentService(IPttAuthService authService, HttpClient httpClient)
        {
            _authService = authService;
            _httpClient = httpClient;
        }

        public async Task<PttKabulResponse> AddShipmentsAsync(IEnumerable<PttShipmentRequest> shipments)
        {
            var creds = _authService.GetCredentials();
            var dosyaAdi = $"dosya_{DateTime.Now:yyyyMMddHHmmss}";

            // XML Namespace tanımları
            XNamespace soapenv = "http://www.w3.org/2003/05/soap-envelope";
            XNamespace kab = "http://kabul.ptt.gov.tr";
            XNamespace xsd = "http://kabul.ptt.gov.tr/xsd";

            // SOAP Envelope oluştur
            var envelope = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "kab", kab),
                new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                new XElement(soapenv + "Body",
                    new XElement(kab + "kabulEkle2",
                        new XElement(kab + "input",
                            shipments.Select(s => new XElement(xsd + "dongu",
                                new XElement(xsd + "aAdres", s.AliciAdres),
                                new XElement(xsd + "aliciAdi", s.AliciAdi),
                                new XElement(xsd + "aliciIlAdi", s.AliciIl),
                                new XElement(xsd + "aliciIlceAdi", s.AliciIlce),
                                new XElement(xsd + "aliciTel", s.AliciTel),
                                new XElement(xsd + "barkodNo", s.Barcode),
                                new XElement(xsd + "agirlik", s.Agirlik),
                                new XElement(xsd + "desi", s.Desi),
                                new XElement(xsd + "ucret", s.Ucret),
                                new XElement(xsd + "musteriReferansNo", s.MusteriReferansNo)
                            )),
                            new XElement(xsd + "dosyaAdi", dosyaAdi),
                            new XElement(xsd + "gonderiTip", "NORMAL"),
                            new XElement(xsd + "gonderiTur", "KARGO"),
                            new XElement(xsd + "kullanici", creds.Username),
                            new XElement(xsd + "musteriId", creds.CustomerId),
                            new XElement(xsd + "sifre", creds.Password)
                        )
                    )
                )
            );

            var content = new StringContent(envelope.ToString(), Encoding.UTF8, "application/soap+xml");
            var response = await _httpClient.PostAsync(Endpoint, content);
            var responseXml = await response.Content.ReadAsStringAsync();

            // Parse Response
            var doc = XDocument.Parse(responseXml);
            XNamespace ns = "http://kabul.ptt.gov.tr/xsd";

            var hataKodu = int.TryParse(doc.Descendants(ns + "hataKodu").FirstOrDefault()?.Value, out var hk) ? hk : -1;
            var aciklama = doc.Descendants(ns + "aciklama").FirstOrDefault()?.Value ?? "";

            var items = doc.Descendants(ns + "dongu").Select(d => new PttKabulItemResult
            {
                Barcode = d.Element(ns + "barkod")?.Value ?? "",
                DonguHataKodu = int.TryParse(d.Element(ns + "donguHataKodu")?.Value, out var dhk) ? dhk : -1,
                DonguAciklama = d.Element(ns + "donguAciklama")?.Value ?? "",
                Success = d.Element(ns + "donguSonuc")?.Value == "true"
            }).ToList();

            return new PttKabulResponse
            {
                HataKodu = hataKodu,
                Aciklama = aciklama,
                DosyaAdi = dosyaAdi, // 🔹 ekle

                Items = items
            };
        }
    }
}
