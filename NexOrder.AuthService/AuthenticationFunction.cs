using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexOrder.AuthService.Application.Authentication.RefreshTokens;
using NexOrder.AuthService.Application.Common;
using NexOrder.AuthService.Application.Services;
using NexOrder.AuthService.Common;
using NexOrder.AuthService.Infrastructure.Services;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using System.Net;

namespace NexOrder.AuthService;

public class AuthenticationFunction
{
    private readonly ILogger<AuthenticationFunction> _logger;
    private readonly IMediator mediator;

    private readonly IAuthenticationService authenticationService;
    public AuthenticationFunction(ILogger<AuthenticationFunction> logger,
        IAuthenticationService authenticationService,
        IMediator mediator)
    {
        _logger = logger;
        this.authenticationService = authenticationService;
        this.mediator = mediator;
    }

    [Function("GenerateToken")]
    [OpenApiOperation(operationId: "GenerateToken", tags: new[] { "example" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UserRequest))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RefreshTokenResponse))]
    public async Task<IActionResult> GenerateToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/authservice/generate-token")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<UserRequest>(requestBody);
        var token = await this.authenticationService.GenerateNewTokenAsync(data.UserId, data.Email);
        return new OkObjectResult(token);
    }

    [Function("RefreshToken")]
    [OpenApiOperation(operationId: "RefreshToken", tags: new[] { "example" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RefreshTokenCommand))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RefreshTokenResponse))]
    public async Task<IActionResult> RefreshToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/authservice/refresh-token")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<RefreshTokenCommand>(requestBody);
        var result = await this.mediator.SendAsync<RefreshTokenCommand, CustomResponse<RefreshTokenResponse>>(data);
        return result.GetResponse();
    }
}