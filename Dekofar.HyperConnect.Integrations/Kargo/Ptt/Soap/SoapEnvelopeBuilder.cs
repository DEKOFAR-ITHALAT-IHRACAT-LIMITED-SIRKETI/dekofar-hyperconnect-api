using Dekofar.HyperConnect.Application.Common.Options;
using Dekofar.HyperConnect.Application.Shipments;
using Dekofar.HyperConnect.Application.Shipments.DTOs;
using System.Xml.Linq;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Soap;

public static class SoapEnvelopeBuilder
{
    public static string BuildCreateShipment(
        PttOptions options,
        CreateShipmentRequest req,
        string barcode)
    {
        var codXml = req.IsCashOnDelivery
            ? $@"
            <kapidaOdeme>
                <tutar>{req.CashOnDeliveryAmount:0.00}</tutar>
                <postaCekiNo>{options.PostCheckAccountNo}</postaCekiNo>
            </kapidaOdeme>"
            : string.Empty;

        return $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
 <soapenv:Body>
  <kabul>
   <musteriNo>{options.CustomerNumber}</musteriNo>
   <sifre>{options.Password}</sifre>
   <barkod>{barcode}</barkod>
   <referansNo>{req.ReferenceId}</referansNo>

   <alici>
     <ad>{req.ReceiverName}</ad>
     <telefon>{req.ReceiverPhone}</telefon>
     <adres>{req.ReceiverAddress}</adres>
     <il>{req.ReceiverCity}</il>
     <ilce>{req.ReceiverDistrict}</ilce>
   </alici>

   {codXml}

  </kabul>
 </soapenv:Body>
</soapenv:Envelope>";
    }

    public static CreateShipmentResult ParseCreateShipment(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);

            // PTT hata dönmüş mü?
            if (doc.Descendants("hata").Any())
            {
                var error = doc.Descendants("hata").First().Value;
                return new CreateShipmentResult
                {
                    Success = false,
                    Error = error
                };
            }

            var barkod = doc.Descendants("barkod").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(barkod))
            {
                return new CreateShipmentResult
                {
                    Success = false,
                    Error = "PTT response invalid"
                };
            }

            return new CreateShipmentResult
            {
                Success = true,
                TrackingNo = barkod
            };
        }
        catch (Exception ex)
        {
            return new CreateShipmentResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}
