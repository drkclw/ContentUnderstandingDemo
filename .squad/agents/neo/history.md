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
