# NexOrder.AuthService

NexOrder.AuthService is an authentication microservice within the NexOrder platform. It is built using Azure Function Apps and responsible for issuing secure JWT tokens to validated consumers. Also responsible for generating refresh tokens when access token gets expired. The service is designed with microservices principles and restricts access through Azure API Management.

---

## Architecture

This solution consists of the following components:

```
NexOrder.AuthService
├── NexOrder.AuthService               # Azure Functions host
├── NexOrder.AuthService.Domain        # Domain entities & business rules
├── NexOrder.AuthService.Application   # Use cases, handlers, interfaces
├── NexOrder.AuthService.Infrastructure# EF Core, DB context, migrations
├── NexOrder.AuthService.Shared        # Shared utilities & common models
```

The architecture supports future expansion of NexOrder microservices operating independently.

## Configuration (App Settings)

The application requires the following configuration values:

| Key | Description |
|-----|-------------|
| AuthSecret | Secret key of generating token |
| ExpirationMinutes | Token expiration duration in minutes |
| Audience | The intended audience the JWT is issued for |
| RefreshTokenExpirationMinutes | Refresh Token expiration duration in minutes |
| Encryption__EncryptionKey | Encryption key |

These settings are defined in Azure Function App → Configuration, and are not committed to source control.

---

## Security and Access Restrictions

- Azure Function App access is restricted by IP, allowing only the outbound IP of Azure API Management.
- Direct access to the function URL is blocked.
- CORS is configured to allow only the API Management origin.
- All requests must go through Azure API Management.

This ensures the authentication service is not publicly accessible and enforces secure access routing.

---

## 🐳 Docker Support

Similar to other NexOrder services, this service supports running locally and deploying via containers.

### Prerequisites

- Docker Desktop (or Docker Engine)
- Docker Compose v2

### 🧱 Dockerfile

A `Dockerfile` is included to build a container image for the service.

Build an image locally:

```bash
docker build -t nexorder-authservice:local .
```

Run the container (example):

```bash
docker run --rm -p 8080:80 \
  -e AuthSecret="<secret-key>" \
  -e ExpirationMinutes="<expirationtimeinminutes>" \
  -e Audience="<audience-name>" \
  -e Issuer="<issuer-name>" \
  nexorder-authservice:local
```

> Note: Actual port bindings and hosting settings depend on how the Function host is configured in the container.
> 

### 🧩 Docker Compose

A `docker-compose.yml` is included to simplify local orchestration.

Start services:

```bash
docker compose up --build
```

Stop services:

```bash
docker compose down
```

### 🔐 Configuration in Containers

For local containers, prefer **environment variables** (or a local `.env` file referenced by Compose) rather than committing secrets.

Common keys:

- `AuthSecret`
- `ExpirationMinutes`
- `Audience`
- `Issuer`
- `RefreshTokenExpirationMinutes`
- `Encryption__EncryptionKey`

---

## 🚢 Deployment

### GitHub Actions

The service supports two deployment workflows using **GitHub Actions** with Azure:

1. **Standard deployment (without containerization)** — builds and deploys the Function App directly
2. **Containerized deployment** — builds a Docker image, pushes to Azure Container Registry, and deploys to Azure Web App for Containers

> **Currently, only the containerized deployment workflow is enabled.**
> 

### Standard Deployment Workflow (Disabled)

When enabled, this workflow:

- Builds & restores the .NET project
- Applies EF Core migrations (controlled pipeline step)
- Deploys directly to Azure Functions

> API Management instances are recreated on demand for cost optimization in non-production environments.
> 

### 🧊 Containerized Deployment Workflow (Active)

This service is deployed as a container to **Azure Web App for Containers**.

High-level flow:

1. Build the Docker image via GitHub Actions
2. Push image to **Azure Container Registry**
3. Configure Azure Web App for Containers to pull and run the image
4. Provide required configuration via **App Settings** (environment variables)

Recommended App Settings (examples):

- `AuthSecret`
- `ExpirationMinutes`
- `Audience`
- `Issuer`
- Any other runtime configuration used by the Function host

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

## Private Nuget Packages

This project depends on the **NexOrder.Framework** package, which is hosted via GitHub Packages. To successfully build the project in a GitHub Actions environment, the workflow must be configured to authenticate with the private NuGet source.

### GitHub Actions Workflow Update

An additional step is required before the `dotnet restore` command to register the private source using the `GITHUB_TOKEN`.

Add the following step to your `.github/workflows/main_nexorder-authservice.yml` file:

```yaml
- name: Add Private NuGet Source
  run: |
    dotnet nuget add source "[https://nuget.pkg.github.com/mitanshu-patel/index.json](https://nuget.pkg.github.com/mitanshu-patel/index.json)" \
      --name "github" \
      --username "${{ github.actor }}" \
      --password "${{ secrets.GITHUB_TOKEN }}" \
      --store-password-in-clear-text

- name: Restore dependencies
  run: dotnet restore
```

### Local Development

For local development, developer will need add new Nuget source with the url of index.json as mentioned above and use PAT(Personal Access Token) created via Developer settings, for more refer ```Readme.md``` of **NexOrder.Framework**.

---

## Summary

| Feature | Implemented |
|--------|-------------|
| Azure Function authentication service | Yes |
| JWT token generation | Yes |
| Refresh token generation | Yes |
| GitHub Actions CI/CD | Yes |
| API Management secured access | Yes |
| CORS restricted to APIM | Yes |
| Public access blocked | Yes |
