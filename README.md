# Content Understanding Demo

A demo application showcasing [Azure AI Content Understanding](https://learn.microsoft.com/azure/ai-services/content-understanding/) — an Azure service for analyzing documents, images, and other content using AI.

## Architecture

```
src/
├── api/                              # .NET 10 Web API (backend)
│   └── ContentUnderstanding.Api/
│       ├── Endpoints/                # Minimal API route mappings
│       ├── Models/                   # Request/response DTOs
│       ├── Services/                 # Azure CU SDK integration
│       └── Program.cs               # Composition root
└── web/                              # Vue 3 + Vite (frontend)
    └── src/
        ├── components/               # Reusable UI components
        ├── views/                    # Page-level views
        ├── services/                 # API client
        └── types/                    # TypeScript type definitions
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- An Azure subscription with a Content Understanding resource
- Azure CLI (`az login`) for DefaultAzureCredential

## Setup

### 1. Configure Azure

Update `src/api/ContentUnderstanding.Api/appsettings.json` with your Azure Content Understanding endpoint and analyzer ID. Alternatively, use user-secrets:

```bash
cd src/api/ContentUnderstanding.Api
dotnet user-secrets set "AzureContentUnderstanding:Endpoint" "https://<your-resource>.cognitiveservices.azure.com/"
dotnet user-secrets set "AzureContentUnderstanding:AnalyzerId" "<your-analyzer-id>"
```

### 2. Run the API

```bash
cd src/api/ContentUnderstanding.Api
dotnet run
```

API starts at `http://localhost:5107`.

### 3. Run the frontend

```bash
cd src/web
npm install
npm run dev
```

Frontend starts at `http://localhost:5173` and proxies `/api` requests to the backend.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/health` | Health check |
| `POST` | `/api/analyze` | Upload a file for analysis (multipart/form-data) |
| `GET` | `/api/analyze/{id}` | Retrieve a previous analysis result |

## Team

Built with [Squad](https://github.com/bradygaster/squad)

| Role | Agent | Owns |
|------|-------|------|
| Lead | Morpheus | Architecture, code review, triage |
| Backend | Neo | .NET API implementation |
| Frontend | Trinity | Vue UI implementation |
| Tester | Oracle | Test coverage and validation |
