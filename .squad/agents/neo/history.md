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

### 2026-04-03 — Invoice analysis wired up (Neo)

Implemented `ContentAnalysisService` using Azure.AI.ContentUnderstanding SDK v1.0.0.

**SDK patterns discovered:**
- `ContentUnderstandingClient.AnalyzeBinaryAsync(WaitUntil.Completed, analyzerId, BinaryData, contentType:, cancellationToken:)` → `Operation<AnalysisResult>`
- `AnalysisResult.Contents` → `IList<AnalysisContent>`, first element is `DocumentContent` for documents
- `AnalysisContent.Fields` → `IDictionary<string, ContentField>` where `ContentField` has `.Value` (object) and `.Confidence` (float?)
- `ContentField` subtypes: `ContentStringField`, `ContentNumberField`, `ContentObjectField`, `ContentArrayField`, `ContentDateTimeOffsetField`
- `Operation.Id` gives the operation ID for result tracking
- `AnalysisResult.CreatedAt` is `DateTimeOffset?`
- No SDK method to retrieve results by operation ID — used `ConcurrentDictionary` in-memory cache

**Multi-scenario design:**
- Config changed from `AnalyzerId` (single) to `Analyzers` (dictionary keyed by scenario name)
- `IContentAnalysisService.AnalyzeAsync` takes `scenario` parameter (default: `"invoice"`)
- `POST /api/analyze?scenario=invoice` — scenario is a query parameter
- `AnalysisResponse` record now includes `Scenario` field

**Key files changed:**
- `Services/ContentAnalysisService.cs` — full SDK integration
- `Services/IContentAnalysisService.cs` — added `scenario` parameter
- `Models/AnalysisRequest.cs` — added `Scenario` to `AnalysisResponse`
- `Endpoints/AnalyzeEndpoints.cs` — added `scenario` query param
- `appsettings.json` — `Analyzers` dictionary config

### 2026-04-04 — ContentField subtype value extraction fixed (Neo)

**Problem:** `ExtractFields` was calling `.Value?.ToString()` on the base `ContentField` class. For complex types like `ContentObjectField`, `.Value` returns `IDictionary<string, ContentField>` boxed as `object`, so `ToString()` produced the type name ("Azure.AI.ContentUnderstanding.ContentObjectField") instead of actual data.

**SDK type hierarchy discovered via reflection (v1.0.0):**
- `ContentField` (base): `Value` (object), `Confidence` (float?), `Spans`, `Sources`
- `ContentStringField`: shadows `Value` → `string`
- `ContentNumberField`: shadows `Value` → `double?`
- `ContentIntegerField`: shadows `Value` → `long?`
- `ContentBooleanField`: shadows `Value` → `bool?`
- `ContentDateTimeOffsetField`: shadows `Value` → `DateTimeOffset?`
- `ContentTimeField`: shadows `Value` → `TimeSpan?`
- `ContentObjectField`: shadows `Value` → `IDictionary<string, ContentField>`, has `Item[string]` indexer
- `ContentArrayField`: shadows `Value` → `IList<ContentField>`, has `Count` and `Item[int]` indexer
- No public `Type` property on `ContentField` base class
- XML docs list `ValueString`, `ValueNumber`, etc. but these DON'T exist as public properties — only `Value` (shadowed per subtype)

**Fix applied:**
- `FieldResult.Value` changed from `string` to `object?` to support native types
- Added `string? Type` parameter (default null) to `FieldResult` for frontend type discrimination
- `ExtractFields` now delegates to `ConvertField` which pattern-matches on all subtypes
- Object fields recursively convert to `Dictionary<string, FieldResult>`
- Array fields recursively convert to `List<FieldResult>`
- Fallback uses `field.Value?.ToString()` with type `"unknown"`

**Key files changed:**
- `Services/ContentAnalysisService.cs` — rewrote ExtractFields, added ConvertField/ConvertObjectFields/ConvertArrayItems
- `Models/AnalysisRequest.cs` — `FieldResult.Value` → `object?`, added `Type` param

### 2026-04-04 — Analysis duration timing added (Neo)

Added `double? AnalysisDurationMs` to `AnalysisResponse`. Used `System.Diagnostics.Stopwatch` to measure only the `_client.AnalyzeBinaryAsync(...)` round-trip — the narrowest meaningful measurement. Logged at `Information` level with `{DurationMs:F0}` format. No new abstractions; two files touched.

### 2026-04-04 — Video summary and transcript extraction implemented (Neo)

**SDK patterns for video content:**
- When `prebuilt-videoSearch` returns results, each item in `result.Contents` is `AudioVisualContent` (must cast from `AnalysisContent`)
- Summary lives in `audioVisualContent.Fields["Summary"].Value?.ToString()` — capital S, on the first content item
- Transcript is a dedicated property: `audioVisualContent.TranscriptPhrases` → `IList<TranscriptPhrase>`
- `TranscriptPhrase` has: `Speaker` (string), `Text` (string), `StartTime` (TimeSpan), `EndTime` (TimeSpan)
- Timing via `phrase.StartTime.TotalSeconds` — NOT via `StartTimeMs` (that's internal/hidden by the SDK customization layer)
- The spec's suggestion of per-item `Fields["transcriptPhrase"]` / `Fields["speakerLabel"]` lookup was incorrect; `TranscriptPhrases` is the right property

**Key corrections vs. Morpheus spec:**
- `StartTimeMs`/`EndTimeMs` are internal (`[CodeGenMember]` suppressed); use `StartTime`/`EndTime` (`TimeSpan`) on `AudioVisualContent` and `TranscriptPhrase`
- Summary field key is `"Summary"` not `"summary"` (capital S)
- No per-content-item transcript fields; all phrases are in `TranscriptPhrases` list on each `AudioVisualContent`

**Files changed:**
- `Models/AnalysisRequest.cs` — added `TranscriptSegment` record; added `Summary` and `Transcript` optional params to `AnalysisResponse`
- `Services/ContentAnalysisService.cs` — added `ExtractVideoContent` method; updated `AnalyzeUrlAsync` to use it

### 2026-04-04 — Video URL analysis implemented (Neo)

Added `POST /api/analyze/url` endpoint for URL-based analysis (primary use: video).

**SDK correction discovered:** The task brief said to use `_client.AnalyzeUrlAsync(...)` — that method does not exist in SDK v1.0.0. URL-based analysis uses the same `AnalyzeAsync` method but with `AnalysisInput` objects:
```csharp
_client.AnalyzeAsync(WaitUntil.Completed, analyzerId,
    inputs: new[] { new AnalysisInput { Uri = new Uri(url) } }, cancellationToken: ct)
```
`AnalyzeBinaryAsync` is for local binary data only; `AnalyzeAsync` with `AnalysisInput.Uri` is for URLs.

**Correct video analyzer ID:** `prebuilt-videoSearch` (not `prebuilt-videoShot`). Video results are `AudioVisualContent` segments — iterate all `result.Contents`, don't assume a single item.

**Key files changed:**
- `Services/IContentAnalysisService.cs` — added `AnalyzeUrlAsync(string url, string scenario, CancellationToken)`
- `Services/ContentAnalysisService.cs` — implemented `AnalyzeUrlAsync` using `AnalysisInput { Uri }`
- `Models/AnalysisRequest.cs` — added `AnalyzeUrlRequest(string Url, string Scenario = "video")`
- `Endpoints/AnalyzeEndpoints.cs` — added `POST /api/analyze/url` with URL validation
- `appsettings.json` — added `"video": "prebuilt-videoSearch"` and filled in other scenarios
