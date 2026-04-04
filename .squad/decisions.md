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

### 8. Test project: xUnit + NSubstitute + WebApplicationFactory
- **Author:** Oracle | **Date:** 2026-04-03 | **Status:** Active
- xUnit 2.9.x (ecosystem default). NSubstitute over Moq (cleaner syntax, active maintenance). `WebApplicationFactory<Program>` for true integration tests through HTTP pipeline. `public partial class Program { }` in Program.cs exposes entry point to test assembly. Shared `ApiFixture` via `IClassFixture` — single test server per class.

### 9. Multi-scenario analyzer config and API shape
- **Author:** Neo | **Date:** 2026-04-03 | **Status:** Active
- `appsettings.json` uses `"Analyzers": { "invoice": "<id>", "utility-bill": "<id>", ... }` dictionary — adding a scenario is one config line. Scenario selected via `POST /api/analyze?scenario=invoice` query param (default: `"invoice"`). No separate endpoints per scenario. In-memory `ConcurrentDictionary` result cache (demo only — production needs durable storage). `AnalysisResponse` includes `Scenario` field.

### 10. FieldResult.Value is object? with Type discriminator
- **Author:** Neo | **Date:** 2026-04-04 | **Status:** Active
- `FieldResult.Value` is `object?` (not `string`) — carries strings, numbers, booleans, dates, nested `Dictionary<string, FieldResult>`, `List<FieldResult>`. Added `string? Type` discriminator ("string", "number", "integer", "boolean", "date", "time", "object", "array", "unknown"). Breaking change to JSON shape. Frontend uses `type` field for rendering logic.

### 11. Scenario cards: utility-bill, receipt, custom
- **Author:** Trinity | **Date:** 2026-04-04 | **Status:** Active
- 3 new scenarios added to `src/web/src/config/scenarios.ts`. IDs: `utility-bill`, `receipt`, `custom`. Each has title, description, icon emoji.

### 12. Document preview panel in AnalyzeView
- **Author:** Trinity | **Date:** 2026-04-04 | **Status:** Active
- Side-by-side grid layout (preview left, results right). Image files render as `<img>`, PDFs as `<iframe>`, others as placeholder. Uses `URL.createObjectURL()` with cleanup via `onBeforeUnmount`. Responsive: stacks vertically below 768px. Preview only shown when results are available.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
