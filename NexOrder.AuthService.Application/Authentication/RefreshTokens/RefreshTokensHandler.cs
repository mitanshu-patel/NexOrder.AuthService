using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NexOrder.AuthService.Application.Common;
using NexOrder.AuthService.Application.Services;
using NexOrder.Framework.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Application.Authentication.RefreshTokens
{
    internal class RefreshTokensHandler : RequestHandlerBase<RefreshTokenCommand, CustomResponse<RefreshTokenResponse>>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<RefreshTokensHandler> logger;

        public RefreshTokensHandler(IAuthenticationService authenticationService,
            ILogger<RefreshTokensHandler> logger)
        {
            this.authenticationService = authenticationService;   
            this.logger = logger;
        }
        protected override async Task<CustomResponse<RefreshTokenResponse>> ExecuteCommandAsync(RefreshTokenCommand command)
        {
            try
            {
                this.logger.LogInformation("RefreshTokensHandler: ExecuteCommandAsync execution started");
                var (refreshTokenResponse, errorMessage) = await this.authenticationService.GenerateJwtAndRefreshTokenAsync(command.Token, command.RefreshToken);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    this.logger.LogError("Error while generating refresh token, error:{error}", errorMessage);
                    return CustomHttpResult.UnAuthorized<RefreshTokenResponse>(errorMessage);
                }

                this.logger.LogInformation("RefreshTokensHandler: ExecuteCommandAsync successfully generated new refresh token");
                return CustomHttpResult.Ok(refreshTokenResponse!);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RefreshTokensHandler: exception occurred with message:{message}", ex.Message);
                throw;
            }
        }

        protected override CustomResponse<RefreshTokenResponse> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<RefreshTokenResponse>();
        }

        protected override IValidator<RefreshTokenCommand> GetValidator()
        {
            var validator = new InlineValidator<RefreshTokenCommand>();
            validator.RuleFor(v=> v.Token).NotEmpty().WithMessage("Token is required.");
            validator.RuleFor(v=> v.RefreshToken).NotEmpty().WithMessage("RefreshToken is required.");
            return validator;
        }
    }
}
