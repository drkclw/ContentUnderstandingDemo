# Project Context

- **Owner:** Sam
- **Project:** Azure Content Understanding Demo — a small demo app showcasing Azure Content Understanding service capabilities
- **Stack:** .NET 10 (backend API), Vue (frontend), Azure Content Understanding
- **Created:** 2026-04-03

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-03 — Project structure scaffolded (Morpheus)

Monorepo created. Your key files:
- `src/web/src/App.vue` — root component
- `src/web/src/views/AnalyzeView.vue` — main analysis view
- `src/web/src/components/FileUpload.vue` — file upload component
- `src/web/src/services/api.ts` — API client
- `src/web/src/types/analysis.ts` — TypeScript types (mirrors backend DTOs)

Stack: Vue 3 + Vite + TypeScript. No router, no Pinia.
Vite proxy: `/api` → `http://localhost:5107`. API contract: `POST /api/analyze`, `GET /api/analyze/{id}`, `GET /api/health`.
