using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;
namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Services
{
    public class DhlKargoShipmentService : IDhlKargoShipmentService
    {
        private readonly IConfiguration _config;
        private readonly IDhlKargoAuthService _authService;

        public DhlKargoShipmentService(IConfiguration config, IDhlKargoAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        /// <summary>
        /// Tek bir shipmentId için ilk status kaydını döndürür.
        /// </summary>
        public async Task<ShipmentStatusResponse> GetShipmentStatusByShipmentIdAsync(string shipmentId)
        {
            var list = await GetShipmentStatusListByShipmentIdAsync(shipmentId);

            if (list == null || list.Count == 0)
                throw new Exception($"DHL Kargo: Gönderi ({shipmentId}) bulunamadı.");

            return list.First();
        }

        /// <summary>
        /// Tek bir shipmentId için tüm status kayıtlarını döndürür.
        /// </summary>
        public async Task<List<ShipmentStatusResponse>> GetShipmentStatusListByShipmentIdAsync(string shipmentId)
        {
            var token = await _authService.GetTokenAsync();

            var client = new RestClient(
                $"https://api.mngkargo.com.tr/mngapi/api/standardqueryapi/getshipmentstatusByShipmentId/{shipmentId}"
            );
            var request = new RestRequest("", Method.Get);

            AddCommonHeaders(request, token.jwt);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL Kargo shipment status hatası: {response.StatusCode} - {response.Content}");

            Console.WriteLine("RAW SHIPMENT STATUS RESPONSE: " + response.Content);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<ShipmentStatusResponse>>(response.Content!, options);

            return list ?? new List<ShipmentStatusResponse>();
        }

        /// <summary>
        /// Tek bir shipmentId için tüm hareketleri (track) döndürür.
        /// </summary>
        public async Task<List<ShipmentTrackResponse>> TrackShipmentByShipmentIdAsync(string shipmentId)
        {
            var token = await _authService.GetTokenAsync();

            var client = new RestClient(
                $"https://api.mngkargo.com.tr/mngapi/api/standardqueryapi/trackshipmentByShipmentId/{shipmentId}"
            );
            var request = new RestRequest("", Method.Get);

            AddCommonHeaders(request, token.jwt);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL Kargo track shipment hatası: {response.StatusCode} - {response.Content}");

            Console.WriteLine("RAW SHIPMENT TRACK RESPONSE: " + response.Content);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<ShipmentTrackResponse>>(response.Content!, options);

            return list ?? new List<ShipmentTrackResponse>();
        }

        /// <summary>
        /// Tek bir shipmentId için detaylı gönderi bilgilerini döndürür.
        /// </summary>
        public async Task<ShipmentDetailResponse> GetShipmentByShipmentIdAsync(string shipmentId)
        {
            var token = await _authService.GetTokenAsync();

            var client = new RestClient(
                $"https://api.mngkargo.com.tr/mngapi/api/standardqueryapi/getshipmentByShipmentId/{shipmentId}"
            );
            var request = new RestRequest("", Method.Get);

            AddCommonHeaders(request, token.jwt);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL Kargo shipment detail hatası: {response.StatusCode} - {response.Content}");

            Console.WriteLine("RAW SHIPMENT DETAIL RESPONSE: " + response.Content);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (string.IsNullOrWhiteSpace(response.Content))
                return new ShipmentDetailResponse();

            // JSON kökü [ ile başlıyorsa listeye deserialize et
            if (response.Content.TrimStart().StartsWith("["))
            {
                var list = JsonSerializer.Deserialize<List<ShipmentDetailResponse>>(response.Content, options);
                return list?.FirstOrDefault() ?? new ShipmentDetailResponse();
            }
            else
            {
                var result = JsonSerializer.Deserialize<ShipmentDetailResponse>(response.Content, options);
                return result ?? new ShipmentDetailResponse();
            }
        }

        /// <summary>
        /// Ortak header ekleme metodu
        /// </summary>
        private void AddCommonHeaders(RestRequest request, string jwt)
        {
            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];

            request.AddHeader("accept", "application/json");
            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);
            request.AddHeader("x-api-version", "1.0");
            request.AddHeader("Authorization", $"Bearer {jwt}");
        }
    }

    // --- DTO’lar ---

    public class ShipmentStatusResponse
    {
        public string? orderId { get; set; }
        public string? referenceId { get; set; }
        public string? shipmentId { get; set; }
        public string? shipmentSerialandNumber { get; set; }
        public string? shipmentDateTime { get; set; }
        public string? shipmentStatus { get; set; }
        public int? shipmentStatusCode { get; set; }
        public string? shipmentStatusExplanation { get; set; }
        public string? statusDateTime { get; set; }
        public string? trackingUrl { get; set; }
        public int? isDelivered { get; set; }
        public string? deliveryDateTime { get; set; }
        public string? deliveryTo { get; set; }
        public string? retrieveShipmentId { get; set; }
    }

    public class ShipmentTrackResponse
    {
        public string? referenceId { get; set; }
        public string? eventSequence { get; set; }
        public string? eventStatus { get; set; }
        public string? eventStatusEn { get; set; }
        public string? eventDateTime { get; set; }
        public string? eventDateTimeFormat { get; set; }
        public string? eventDateTimezone { get; set; }
        public string? eventDateTime2 { get; set; }
        public string? eventDateTime2Format { get; set; }
        public string? eventDateTime2zone { get; set; }
        public string? location { get; set; }
        public string? country { get; set; }
        public string? locationAddress { get; set; }
        public string? locationPhone { get; set; }
        public string? deliveryDateTime { get; set; }
        public string? deliveryTo { get; set; }
        public string? description { get; set; }
        public string? pieceTotal { get; set; }
    }

    public class ShipmentDetailResponse
    {
        public ShipmentInfo? shipment { get; set; }
        public List<ShipmentPiece>? shipmentPieceList { get; set; }
        public Shipper? shipper { get; set; }
        public Recipient? recipient { get; set; }
    }

    public class ShipmentInfo
    {
        public string? shipmentServiceType { get; set; }
        public string? packagingType { get; set; }
        public string? paymentType { get; set; }
        public string? deliveryType { get; set; }
        public string? referenceId { get; set; }
        public string? shipmentId { get; set; }
        public string? shipmentSerialNumber { get; set; }
        public string? shipmentNumber { get; set; }
        public string? shipmentDateTime { get; set; }
        public int? pieceCount { get; set; }
        public decimal? totalKg { get; set; }
        public decimal? totalDesi { get; set; }
        public decimal? totalKgDesi { get; set; }
        public decimal? total { get; set; }
        public decimal? kdv { get; set; }
        public decimal? finalTotal { get; set; }
        public int? shipmentStatusCode { get; set; }
        public int? isMarketPlaceShipment { get; set; }
        public int? isMarketPlacePays { get; set; }
        public string? shipperBranch { get; set; }
        public string? billOfLandingId { get; set; }
        public int? isCOD { get; set; }
        public string? codAmount { get; set; }
        public string? content { get; set; }
        public string? estimatedDeliveryDate { get; set; }
        public int? isDelivered { get; set; }
    }

    public class ShipmentPiece
    {
        public int? numberOfPieces { get; set; }
        public int? kgDesi { get; set; }
        public string? barcode { get; set; }
        public int? desi { get; set; }
        public int? kg { get; set; }
        public string? content { get; set; }
    }

    public class Shipper
    {
        public int? customerId { get; set; }
        public string? refCustomerId { get; set; }
        public string? city { get; set; }
        public string? district { get; set; }
        public string? address { get; set; }
        public string? bussinessPhoneNumber { get; set; }
        public string? email { get; set; }
        public string? taxOffice { get; set; }
        public string? taxNumber { get; set; }
        public string? fullName { get; set; }
        public string? homePhoneNumber { get; set; }
        public string? mobilePhoneNumber { get; set; }
    }

    public class Recipient
    {
        public string? refCustomerId { get; set; }
        public string? city { get; set; }
        public string? district { get; set; }
        public string? address { get; set; }
        public string? bussinessPhoneNumber { get; set; }
        public string? email { get; set; }
        public string? taxOffice { get; set; }
        public string? taxNumber { get; set; }
        public string? fullName { get; set; }
        public string? homePhoneNumber { get; set; }
        public string? mobilePhoneNumber { get; set; }
    }
}
