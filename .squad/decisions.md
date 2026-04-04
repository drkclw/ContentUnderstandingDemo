# Squad Decisions

## Active Decisions

### 1. Monorepo with `src/api` and `src/web`
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- Two projects under `src/`. One repo, one PR flow, one CI pipeline.

### 2. .NET 10 Minimal APIs (not controllers)
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- Demo has ~3 endpoints. Minimal APIs in `Endpoints/` static classes.

### 3. Vue 3 + Vite + TypeScript
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- Standard modern Vue stack. No router (single view). No Pinia (no complex state).

### 4. Vite proxy for dev, CORS for safety
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- Frontend `:5173` proxies `/api` to `:5107`. Backend CORS as fallback.

### 5. Azure DefaultAzureCredential (no keys in config)
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- Works with `az login` locally, managed identity in prod. No API keys.

### 6. Service abstraction for testability
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- `IContentAnalysisService` wraps Azure SDK. Clear contract + test seam.

### 7. Solution file (.slnx) at repo root
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- .NET 10 `.slnx` format. Single solution, single project.

### API Contract
- **Author:** Morpheus | **Date:** 2026-04-03 | **Status:** Active
- `POST /api/analyze` — multipart/form-data, field: "file"
- `GET /api/analyze/{id}` — retrieve result by ID
- `GET /api/health` — health check

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
