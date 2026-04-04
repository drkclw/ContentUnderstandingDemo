# Project Context

- **Owner:** Sam
- **Project:** Azure Content Understanding Demo — a small demo app showcasing Azure Content Understanding service capabilities
- **Stack:** .NET 10 (backend API), Vue (frontend), Azure Content Understanding
- **Created:** 2026-04-03

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-03 — Project Structure Created

- **Architecture:** Monorepo — `src/api/` (.NET 10 minimal API) + `src/web/` (Vue 3 + Vite + TS)
- **Solution format:** `.slnx` (new .NET 10 default, not `.sln`)
- **Azure SDK:** `Azure.AI.ContentUnderstanding` 1.0.0 (GA). Originally tried beta.2 but GA was available.
- **Auth pattern:** `DefaultAzureCredential` — no keys in config files
- **API design:** 3 endpoints via minimal APIs in `Endpoints/` static extension methods
- **Service layer:** `IContentAnalysisService` interface — clear contract for Neo, mockable for Oracle
- **Frontend proxy:** Vite proxies `/api` → `http://localhost:5107` in dev
- **Key paths:** Program.cs (composition root), AnalyzeEndpoints.cs (routes), ContentAnalysisService.cs (SDK integration — has TODOs for Neo)
- **No over-engineering:** No router, no state management library, no controller pattern — demo scope only
