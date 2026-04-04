# Project Context

- **Owner:** Sam
- **Project:** Azure Content Understanding Demo — a small demo app showcasing Azure Content Understanding service capabilities
- **Stack:** .NET 10 (backend API), Vue (frontend), Azure Content Understanding
- **Created:** 2026-04-03

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-03 — Project structure scaffolded (Morpheus)

Monorepo created. Your key files:
- `src/api/ContentUnderstanding.Api/Program.cs` — composition root
- `src/api/.../Endpoints/AnalyzeEndpoints.cs` — route definitions
- `src/api/.../Services/IContentAnalysisService.cs` — service contract (implement this)
- `src/api/.../Services/ContentAnalysisService.cs` — Azure SDK integration (TODO)
- `src/api/.../Models/AnalysisRequest.cs` — DTOs

API contract: `POST /api/analyze`, `GET /api/analyze/{id}`, `GET /api/health`.
Auth: `DefaultAzureCredential`. Minimal APIs, no controllers.
