<script setup lang="ts">
import { ref } from 'vue';
import FileUpload from '../components/FileUpload.vue';
import FieldRow from '../components/FieldRow.vue';
import { analyzeFile } from '../services/api';
import type { AnalysisResponse } from '../types/analysis';

const props = defineProps<{ scenario?: string }>();

const isLoading = ref(false);
const error = ref<string | null>(null);
const result = ref<AnalysisResponse | null>(null);
const selectedFileName = ref<string | null>(null);

async function onFileSelected(file: File) {
  error.value = null;
  result.value = null;
  selectedFileName.value = file.name;
  isLoading.value = true;

  try {
    result.value = await analyzeFile(file, props.scenario);
  } catch (err: any) {
    error.value = err.message ?? 'An unexpected error occurred.';
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="analyze-view">
    <FileUpload @selected="onFileSelected" />

    <div v-if="selectedFileName" class="status-bar">
      <span>{{ selectedFileName }}</span>
      <span v-if="isLoading" class="loading">Analyzing...</span>
    </div>

    <div v-if="error" class="error-card">
      <p>{{ error }}</p>
    </div>

    <div v-if="result" class="result-card">
      <h2>Analysis Result</h2>
      <dl class="meta">
        <dt>ID</dt><dd>{{ result.id }}</dd>
        <dt>Status</dt><dd>{{ result.status }}</dd>
        <dt>Scenario</dt><dd>{{ result.scenario ?? '—' }}</dd>
        <dt>File</dt><dd>{{ result.fileName }}</dd>
        <dt>Analyzed</dt><dd>{{ new Date(result.analyzedAt).toLocaleString() }}</dd>
      </dl>

      <div v-if="result.fields && Object.keys(result.fields).length" class="fields">
        <h3>Extracted Fields</h3>
        <table>
          <thead>
            <tr>
              <th>Field</th>
              <th>Value</th>
              <th>Confidence</th>
            </tr>
          </thead>
          <tbody>
            <FieldRow
              v-for="(field, key) in result.fields"
              :key="key"
              :field="field"
              :field-key="String(key)"
              :depth="0"
            />
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>

<style scoped>
.analyze-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.status-bar {
  display: flex;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  background: var(--color-surface, #fff);
  border-radius: var(--radius, 8px);
  font-size: 0.9rem;
}

.loading {
  color: var(--color-primary, #0078d4);
  font-weight: 500;
}

.error-card {
  padding: 1rem;
  background: #fef0f0;
  border: 1px solid #e8c4c4;
  border-radius: var(--radius, 8px);
  color: #a80000;
}

.result-card {
  padding: 1.5rem;
  background: var(--color-surface, #fff);
  border-radius: var(--radius, 8px);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.result-card h2 {
  font-size: 1.25rem;
  margin-bottom: 1rem;
}

.meta {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 0.25rem 1rem;
  margin-bottom: 1.5rem;
}

.meta dt {
  font-weight: 600;
  color: var(--color-text-secondary, #616161);
}

.fields h3 {
  font-size: 1rem;
  margin-bottom: 0.75rem;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  font-weight: 600;
  color: var(--color-text-secondary, #616161);
  font-size: 0.85rem;
}

.group-header td {
  font-weight: 600;
  background: #f9f9fb;
}
</style>
