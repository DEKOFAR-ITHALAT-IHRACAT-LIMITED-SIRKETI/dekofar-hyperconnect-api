using Dekofar.HyperConnect.Application.Common.Options;
using Dekofar.HyperConnect.Application.Shipments;
using Dekofar.HyperConnect.Application.Shipments.DTOs;
using System.Text;

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
        // Basit örnek – prod’da XML node parsing yapılmalı
        if (xml.Contains("<hata>"))
            return new() { Success = false, Error = "PTT error" };

        var trackingNo = Extract(xml, "barkod");

        return new()
        {
            Success = true,
            TrackingNo = trackingNo
        };
    }

    private static string Extract(string xml, string node)
    {
        var start = xml.IndexOf($"<{node}>") + node.Length + 2;
        var end = xml.IndexOf($"</{node}>");
        return xml.Substring(start, end - start);
    }
}
