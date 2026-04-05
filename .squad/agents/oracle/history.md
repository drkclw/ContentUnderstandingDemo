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

### 2026-04-03 — Test project created (Oracle)

Test project: `src/api/ContentUnderstanding.Api.Tests/`
- xUnit 2.9.x + NSubstitute 5.3 + Microsoft.AspNetCore.Mvc.Testing
- `ApiFixture` (`WebApplicationFactory<Program>`) swaps `IContentAnalysisService` with NSubstitute mock
- `Program.cs` uses `public partial class Program { }` to expose the entry point to the test assembly
- 6 tests: 4 analyze-endpoint (happy path, empty file, oversized file, GET by ID found/not-found), 1 health endpoint
- All green on first run
- Solution file updated to include test project in `/src/api/` folder

### 2026-04-04 — Tests updated for FieldResult model changes (Oracle)

`FieldResult` changed from 3-param `(Name, Value, Confidence)` to 4-param `(Name, object? Value, Confidence, Type)`.
- Updated existing `Post_Analyze_WithValidFile_ReturnsOkWithResponse` test: added `"string"` type to FieldResult constructor
- Added `Post_Analyze_WithComplexFields_SerializesNestedTypesCorrectly` test: covers object fields (nested dictionary of FieldResult), array fields (list of FieldResult), plus number and boolean value types — verifies correct serialization through the API endpoint
- 7 tests total, all passing

### 2026-04-04 — Added scenario-specific endpoint tests

Added `Post_Analyze_WithScenario_ReturnsOkWithMatchingScenario` — a parameterized `[Theory]` test covering utility-bill, receipt, and custom scenarios. Uses `[InlineData]` for each scenario with expected ID, file name. Verifies status code, response fields, and scenario matching. 8 tests total now.

### 2026-04-04 — Updated video URL tests for Summary + Transcript fields (Oracle)

Updated `AnalyzeUrlEndpointTests` to cover new `Summary` and `Transcript` fields on `AnalysisResponse` (spec from Morpheus decision inbox). Changes:
- `Post_AnalyzeUrl_WithValidUrl_ReturnsOkWithResponse` — extended mock with `Summary` + 2-item `Transcript`; added assertions for both
- `Post_AnalyzeUrl_WithVideoScenario_ReturnsSummaryAndTranscript` — 3-segment transcript, verifies summary text, count, first speaker, non-zero StartTimeSeconds on second segment
- `Post_AnalyzeUrl_WithNoTranscript_ReturnsNullTranscript` — summary present but Transcript explicitly null; verifies null assertion

Build fails with `CS0246`/`CS1061` for `TranscriptSegment` and `Summary`/`Transcript` on `AnalysisResponse` — expected, awaiting Neo's model extension. Tests are structurally correct and will go green once Neo's PR lands.

### 2026-04-04 — Added video URL endpoint tests (Oracle)

Created `AnalyzeUrlEndpointTests` in a new file for `POST /api/analyze/url`. 4 tests:
- `Post_AnalyzeUrl_WithValidUrl_ReturnsOkWithResponse` — happy path, valid https URL + "video" scenario, verifies 200 and response fields
- `Post_AnalyzeUrl_WithEmptyUrl_ReturnsBadRequest` — empty string URL yields 400
- `Post_AnalyzeUrl_WithMissingUrl_ReturnsBadRequest` — null URL field yields 400
- `Post_AnalyzeUrl_WithVideoScenario_ReturnsMatchingScenario` — verifies Scenario field in response equals "video"
By the time tests were written, Neo had already added `AnalyzeUrlAsync` to `IContentAnalysisService` — no TODO needed. Build fails on the production implementation (Azure SDK `ContentUnderstandingClient` doesn't have `AnalyzeUrlAsync` yet) — not a test-code issue. Tests will be green once Neo's implementation lands.
