# Project Context

- **Owner:** Sam
- **Project:** Azure Content Understanding Demo — a small demo app showcasing Azure Content Understanding service capabilities
- **Stack:** .NET 10 (backend API), Vue (frontend), Azure Content Understanding
- **Created:** 2026-04-03

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-03 — Project structure scaffolded (Morpheus)

Monorepo created. Test-relevant info:
- `IContentAnalysisService` interface at `src/api/.../Services/IContentAnalysisService.cs` — mockable seam for testing
- Backend: .NET 10 minimal APIs, `src/api/ContentUnderstanding.Api/`
- Frontend: Vue 3 + Vite + TypeScript, `src/web/`
- API contract: `POST /api/analyze`, `GET /api/analyze/{id}`, `GET /api/health`

Service abstraction chosen specifically for testability. Plan test projects accordingly.
