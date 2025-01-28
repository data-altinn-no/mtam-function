using System.Net;
using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.NewtonsoftJson;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MtamFunction.Models;
using MtamFunction.Services;

namespace MtamFunction;

public class Function(IMtamService mtamService)
{
    // For getting mtam messages, filtered by timestamp
    [Function("mtam")]
    public async Task<HttpResponseData> Mtam(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var from = query["fromDate"];
        DateTime? fromDate = from is not null ? DateTime.Parse(from) : null;
        var mtams = mtamService.Get(fromDate);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(mtams);
        return response;
    }
    
    // For getting a specific mtam message
    [Function("MtamSingle")]
    public async Task<HttpResponseData> MtamSingle(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route="mtam/{id}")] HttpRequestData req,
        string id)
    {
        var mtam = mtamService.GetById(id);
        if (mtam is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            return notFound;
        }
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(mtam);
        return response;
    }
    
    // For creating mtam messages that can later be fetched by tilda
    [Function("MtamCreate")]
    public async Task<HttpResponseData> MtamCreate(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route="mtam/create")] HttpRequestData req,
        string id)
    {
        var body = await req.ReadAsStringAsync();
        if (body is null)
        {
            var faultyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return faultyResponse;
        }
        var mtam = JsonSerializer.Deserialize<MtamSourceMessage>(body);
        if (mtam?.MessageContent is null || !MessageTypeIsValid(mtam.MessageContent.MessageType))
        {
            var faultyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return faultyResponse;
        }

        mtam.Timestamp = DateTime.UtcNow;
        mtamService.Create(mtam);
        var response = req.CreateResponse(HttpStatusCode.OK);
        return response;
    }
    
    // For receiving mtam Messages from "another" source
    [Function("mtamPost")]
    public async Task<HttpResponseData> MtamPost(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mtam")] HttpRequestData req)
    {
        CloudEventFormatter formatter = new JsonEventFormatter();
        var cloudEvent = await formatter.DecodeStructuredModeMessageAsync(req.Body, new ContentType("application/json"), []);
        if (cloudEvent is null)
        {
            var faultyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return faultyResponse;
        }

        mtamService.AddCloudEvent(cloudEvent);
        var response = req.CreateResponse(HttpStatusCode.OK);
        return response;
    }
    
    // For getting received cloud events
    [Function("cloudEvents")]
    public async Task<HttpResponseData> CloudEvents(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var cloudEvents = mtamService.GetCloudEvents().ToList();
        var response = req.CreateResponse(HttpStatusCode.OK);
        CloudEventFormatter formatter = new JsonEventFormatter();
        var bytes = formatter.EncodeBatchModeMessage(cloudEvents, out var contentType);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteBytesAsync(bytes.ToArray());
        return response;
    }

    private static bool MessageTypeIsValid(string? type)
    {
        return type is "varsel-om-rapport" or "varsel-om-koordinering" or "varsel-fritekst";
    }
}