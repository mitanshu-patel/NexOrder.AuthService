# NexOrder.AuthService

NexOrder.AuthService is an authentication microservice within the NexOrder platform. It is built using Azure Function Apps and responsible for issuing secure JWT tokens to validated consumers. The service is designed with microservices principles and restricts access through Azure API Management.

---

## Architecture

This solution consists of the following components:

| Component | Description |
|----------|-------------|
| NexOrder.AuthService | Azure Function App responsible for authentication endpoints |
| NexOrder.AuthService.Infrastructure | Class library responsible for token creation and authentication logic |
| Azure API Management | Acts as the secured gateway for accessing the function |
| GitHub Actions | Automates deployment to Azure Function App |
| CORS & Access Restrictions | Ensures only APIM can access the function app |

The architecture supports future expansion of NexOrder microservices operating independently.

---

## Project Structure

NexOrder.AuthService
│
├── NexOrder.AuthService                # Azure Function App
│   └── Contains function triggers and endpoints for authentication
│
└── NexOrder.AuthService.Infrastructure # Class library
    ├── Implements JWT token generation
    ├── Handles token expiration and validation
    └── Manages audience and issuer logic

---

## Configuration (App Settings)

The application requires the following configuration values:

| Key | Description |
|-----|-------------|
| AuthSecret | Secret key of generating token |
| ExpirationMinutes | Token expiration duration in minutes |
| Audience | The intended audience the JWT is issued for |
| Issuer | The authority issuing the token |

These settings are defined in Azure Function App → Configuration, and are not committed to source control.

---

## Security and Access Restrictions

- Azure Function App access is restricted by IP, allowing only the outbound IP of Azure API Management.
- Direct access to the function URL is blocked.
- CORS is configured to allow only the API Management origin.
- All requests must go through Azure API Management.

This ensures the authentication service is not publicly accessible and enforces secure access routing.

---

## Deployment (GitHub Actions)

The Azure Function App is deployed using GitHub Actions:

- The Azure portal is configured to reference the GitHub repository.
- Code push triggers the workflow pipeline.
- The function is automatically built and deployed to Azure.

This supports automated CI/CD for incremental deployments.

---

## API Management Integration

- API is added to API Management by referencing the deployed Azure Function App.
- Inbound policy includes CORS configuration.
- API Management becomes the only entry point for clients consuming this authentication service.

---

## Local Development

Requirements:

- .NET SDK (matching project target version)
- Azure Functions Core Tools
- A local appsettings.json file with required configuration keys

Requests executed locally will bypass access restrictions; however, once deployed, requests must flow through API Management.

---

## Summary

| Feature | Implemented |
|--------|-------------|
| Azure Function authentication service | Yes |
| JWT token generation | Yes |
| GitHub Actions CI/CD | Yes |
| API Management secured access | Yes |
| CORS restricted to APIM | Yes |
| Public access blocked | Yes |
