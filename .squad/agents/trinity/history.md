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
