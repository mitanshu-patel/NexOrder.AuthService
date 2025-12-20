using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexOrder.AuthService.Common;
using NexOrder.AuthService.Infrastructure.Services;
using System.Net;

namespace NexOrder.AuthService;

public class AuthenticationFunction
{
    private readonly ILogger<AuthenticationFunction> _logger;

    private readonly IAuthenticationService authenticationService;
    public AuthenticationFunction(ILogger<AuthenticationFunction> logger,
        IAuthenticationService authenticationService)
    {
        _logger = logger;
        this.authenticationService = authenticationService;
    }

    [Function("GenerateToken")]
    [OpenApiOperation(operationId: "GenerateToken", tags: new[] { "example" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UserRequest))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string))]
    public async Task<IActionResult> GenerateToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/authservice/generate-token")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<UserRequest>(requestBody);
        var token = this.authenticationService.IssueJWT(data.Email);
        return new OkObjectResult(new {Token = token});
    }
}