using Dekofar.HyperConnect.Application.Common.Options;
using Dekofar.HyperConnect.Application.Shipments;
using Dekofar.HyperConnect.Application.Shipments.DTOs;
using Dekofar.HyperConnect.Application.Shipments.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Soap;
using Microsoft.Extensions.Options;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Providers;

public class PttShipmentProvider : IShipmentProvider
{
    private readonly PttOptions _options;
    private readonly IBarcodeService _barcodeService;
    private readonly HttpClient _http;

    public PttShipmentProvider(
        IOptions<PttOptions> options,
        IBarcodeService barcodeService,
        HttpClient http)
    {
        _options = options.Value;
        _barcodeService = barcodeService;
        _http = http;
    }

    public async Task<CreateShipmentResult> CreateAsync(
        CreateShipmentRequest request)
    {
        try
        {
            var barcode = await _barcodeService.NextAsync();

            var soapXml = SoapEnvelopeBuilder.BuildCreateShipment(
                _options,
                request,
                barcode
            );

            var endpoint = _options.Environment == "Production"
                ? _options.Endpoints.Production
                : _options.Endpoints.Test;

            var content = new StringContent(
                soapXml,
                Encoding.UTF8,
                "text/xml");

            var response = await _http.PostAsync(endpoint, content);
            var responseXml = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Fail("PTT HTTP error");

            return SoapEnvelopeBuilder.ParseCreateShipment(responseXml);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    // Şimdilik soft-disable (exception yok!)
    public Task CancelAsync(string referenceId)
        => Task.CompletedTask;

    public Task<TrackingResult> TrackAsync(string trackingNo)
        => Task.FromResult(new TrackingResult
        {
            Success = false,
            Error = "Tracking not implemented yet"
        });

    private static CreateShipmentResult Fail(string error)
        => new() { Success = false, Error = error };
}
