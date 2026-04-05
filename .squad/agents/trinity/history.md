# Project Context

- **Owner:** Sam
- **Project:** Azure Content Understanding Demo — a small demo app showcasing Azure Content Understanding service capabilities
- **Stack:** .NET 10 (backend API), Vue (frontend), Azure Content Understanding
- **Created:** 2026-04-03

## Learnings

### 2026-04-04 — Added VideoResult component + video result routing in AnalyzeView

Added `TranscriptSegment` interface and optional `summary`/`transcript` fields to `AnalysisResponse` in `types/analysis.ts`. Created `src/web/src/components/VideoResult.vue` — renders a summary card and a scrollable transcript list with monospace timestamp, speaker chip (`#e8f0fe / #1a73e8`), and alternating row shading. Updated `AnalyzeView.vue`: wrapped the `v-if="result"` block in `<template v-if="result">`, gates on `result.scenario === 'video'` to render `<VideoResult>` full-width (no preview grid), falls through to the existing `.result-layout` grid otherwise. TypeScript compiles clean with `vue-tsc --noEmit`.

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-04 — Added video playback panel to VideoResult.vue

Added `videoRef` template ref and `currentTime` ref to sync player state. `seekTo(seconds)` sets `currentTime` and calls `play()`. `activeSegmentIdx` computed walks the transcript array to find the last segment whose `startTimeSeconds <= currentTime`. Template: video player card inserted above summary card with `@timeupdate` binding. Transcript rows made clickable (`@click="seekTo"`) with `:class="{ active: idx === activeSegmentIdx }"` and `.transcript-row.active` highlight (blue left-border + light-blue background). Needed `const props = defineProps<...>()` (not bare `defineProps`) so the computed could reference `props.result.transcript`. TypeScript compiles clean.

### 2026-04-04 — Added analysis duration timing to AnalyzeView

Added `analysisDuration` ref (reset to `null` on new file submission) that captures `performance.now()` before/after the `analyzeFile` call. The elapsed time is computed in the `finally` block so it records duration even on error. Displayed as a "Duration" row in the `<dl class="meta">` block. TypeScript compiles clean.

### 2026-04-03 — Project structure scaffolded (Morpheus)

Monorepo created. Your key files:
- `src/web/src/App.vue` — root component
- `src/web/src/views/AnalyzeView.vue` — main analysis view
- `src/web/src/components/FileUpload.vue` — file upload component
- `src/web/src/services/api.ts` — API client
- `src/web/src/types/analysis.ts` — TypeScript types (mirrors backend DTOs)

Stack: Vue 3 + Vite + TypeScript. No router, no Pinia.
Vite proxy: `/api` → `http://localhost:5107`. API contract: `POST /api/analyze`, `GET /api/analyze/{id}`, `GET /api/health`.

### 2026-04-04 — Updated types and AnalyzeView for polymorphic FieldResult

Neo changed the backend `FieldResult` so `Value` is `object?` (not just `string`) and added a `Type` discriminator. Updated:
- **`types/analysis.ts`**: Added `FieldType` union, `FieldValue` union (string | number | boolean | null | nested object | array), `type` field on `FieldResult`, and `scenario` on `AnalysisResponse`.
- **`views/AnalyzeView.vue`**: Template now handles simple, object, and array field values with nested rendering (indented child rows). Added helper functions (`formatValue`, `isObjectValue`, `isArrayValue`, `objectEntries`, `arrayItems`, `formatConfidence`). Scenario displayed in result metadata.
- TypeScript compiles clean with `vue-tsc --noEmit`.

### 2026-04-04 — Fixed array-of-objects rendering in AnalyzeView

Array items that are objects (e.g., invoice LineItems) were showing `[object Object]`. Fixed by adding template branching inside the array `v-for`: object items now render a sub-header row followed by indented child field rows. Simple array items still render as before. Added `.array-item-header` and `.deep-nested-name` CSS classes for the deeper nesting level. TypeScript compiles clean.

### 2026-04-04 — Recursive FieldRow component + collapsible arrays

Problem: AnalyzeView had hard-coded 2-level nesting; deeply nested structures (e.g., `LineItems[0].VendorAddress.Street`) showed `[object Object]`. Solution:
- **Created `src/web/src/components/FieldRow.vue`** — a self-recursive component that renders any `FieldResult` at arbitrary depth. Handles simple values, objects (renders children recursively), and arrays (collapsible with chevron toggle). Indentation is depth-driven via computed `padding-left`. Arrays of objects default to collapsed; simple arrays default to expanded.
- **Refactored `AnalyzeView.vue`** — replaced the monolithic `<template v-for>` blocks with a single `<FieldRow>` per top-level field. Removed helper functions (`formatValue`, `isObjectValue`, `isArrayValue`, `objectEntries`, `arrayItems`, `formatConfidence`) and old nesting CSS classes from AnalyzeView; those now live in FieldRow.
- TypeScript compiles clean with `vue-tsc --noEmit`.

### 2026-04-04 — Added scenario cards and document preview

Added 3 new scenario cards to `src/web/src/config/scenarios.ts`: utility-bill (💡), receipt (🛒), custom (🔍). These join the existing invoice scenario. Each has id, title, description, icon.

Added document preview panel to `src/web/src/views/AnalyzeView.vue`:
- Side-by-side grid layout: preview (left) + results (right)
- Supports image files (png, jpg, jpeg, bmp, tiff), PDF (iframe), and placeholder for other types
- Uses `URL.createObjectURL()` with proper cleanup via `onBeforeUnmount`
- Responsive: stacks vertically below 768px
- Preview only shows when results are available (`v-if="result"`)

### 2026-04-04 — Enlarged document preview panel (A+B+C layout combo)

CSS-only changes to `AnalyzeView.vue` approved by Sam:
- Grid split changed from `1fr 1fr` to `2fr 1fr` — preview gets 66% of width, results panel 34%.
- PDF iframe height changed from `500px` to `70vh` — fills the visible screen on any monitor.
- Preview panel made sticky (`position: sticky; top: 1rem`) so the document stays pinned while the user scrolls a long fields table. Added `max-height: calc(100vh - 2rem); overflow-y: auto` to prevent viewport overflow.

### 2026-04-04 — Added Video scenario with URL input

Added `inputType?: 'file' | 'url'` discriminator to `ScenarioDefinition` in `scenarios.ts`. New `video` scenario uses `inputType: 'url'`. Added `analyzeUrl()` to `api.ts` (POSTs JSON to `/api/analyze/url`). Updated `AnalyzeView.vue`: computed `isUrlScenario` swaps `<FileUpload>` for a `.url-input-group` (text input + submit button). Status bar shows truncated URL for URL scenarios. Preview panel stays hidden for URL submissions (already gated on `previewUrl`). TypeScript compiles clean.

