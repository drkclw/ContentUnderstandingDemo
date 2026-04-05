<script setup lang="ts">
import { ref, computed, watch, onBeforeUnmount } from 'vue';
import FileUpload from '../components/FileUpload.vue';
import FieldRow from '../components/FieldRow.vue';
import VideoResult from '../components/VideoResult.vue';
import { analyzeFile, analyzeUrl } from '../services/api';
import { scenarios } from '../config/scenarios';
import type { AnalysisResponse } from '../types/analysis';

const props = defineProps<{ scenario?: string }>();

const isLoading = ref(false);
const error = ref<string | null>(null);
const result = ref<AnalysisResponse | null>(null);
const selectedFileName = ref<string | null>(null);
const previewUrl = ref<string | null>(null);
const selectedFile = ref<File | null>(null);
const analysisDuration = ref<number | null>(null);
const videoUrl = ref('');
const selectedUrl = ref<string | null>(null);

const scenarioDef = computed(() => scenarios.find(s => s.id === props.scenario));
const isUrlScenario = computed(() => scenarioDef.value?.inputType === 'url');

const imageExtensions = ['png', 'jpg', 'jpeg', 'bmp', 'tiff'];

const fileExtension = computed(() => {
  if (!selectedFileName.value) return '';
  return selectedFileName.value.split('.').pop()?.toLowerCase() ?? '';
});

const previewType = computed<'image' | 'pdf' | 'other'>(() => {
  if (imageExtensions.includes(fileExtension.value)) return 'image';
  if (fileExtension.value === 'pdf') return 'pdf';
  return 'other';
});

function revokePreview() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value);
    previewUrl.value = null;
  }
}

watch(selectedFile, () => {
  revokePreview();
  if (selectedFile.value) {
    previewUrl.value = URL.createObjectURL(selectedFile.value);
  }
});

onBeforeUnmount(revokePreview);

async function onFileSelected(file: File) {
  error.value = null;
  result.value = null;
  analysisDuration.value = null;
  selectedFileName.value = file.name;
  selectedFile.value = file;
  isLoading.value = true;

  const t0 = performance.now();
  try {
    result.value = await analyzeFile(file, props.scenario);
  } catch (err: any) {
    error.value = err.message ?? 'An unexpected error occurred.';
  } finally {
    analysisDuration.value = performance.now() - t0;
    isLoading.value = false;
  }
}

async function onUrlSubmit() {
  if (!videoUrl.value) return;
  error.value = null;
  result.value = null;
  analysisDuration.value = null;
  selectedUrl.value = videoUrl.value;
  isLoading.value = true;

  const t0 = performance.now();
  try {
    result.value = await analyzeUrl(videoUrl.value, props.scenario ?? 'video');
  } catch (err: any) {
    error.value = err.message ?? 'An unexpected error occurred.';
  } finally {
    analysisDuration.value = performance.now() - t0;
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="analyze-view">
    <FileUpload v-if="!isUrlScenario" @selected="onFileSelected" />
    <div v-else class="url-input-group">
      <input
        v-model="videoUrl"
        type="url"
        placeholder="https://example.com/video.mp4"
        class="url-input"
        @keyup.enter="onUrlSubmit"
      />
      <button class="url-submit-btn" :disabled="!videoUrl || isLoading" @click="onUrlSubmit">
        {{ isLoading ? 'Analyzing…' : 'Analyze' }}
      </button>
    </div>

    <div v-if="selectedFileName || selectedUrl" class="status-bar">
      <span v-if="!isUrlScenario">{{ selectedFileName }}</span>
      <span v-else class="status-url" :title="selectedUrl ?? ''">{{ selectedUrl }}</span>
      <span v-if="isLoading" class="loading">Analyzing...</span>
    </div>

    <div v-if="error" class="error-card">
      <p>{{ error }}</p>
    </div>

    <template v-if="result">
      <VideoResult v-if="result.scenario === 'video'" :result="result" :video-src="selectedUrl ?? undefined" />
      <div v-else class="result-layout">
      <aside v-if="previewUrl" class="preview-panel">
        <h3>Document Preview</h3>
        <img
          v-if="previewType === 'image'"
          :src="previewUrl"
          :alt="selectedFileName ?? 'Document preview'"
          class="preview-image"
        />
        <iframe
          v-else-if="previewType === 'pdf'"
          :src="previewUrl"
          class="preview-pdf"
          title="PDF preview"
        />
        <div v-else class="preview-placeholder">
          <span class="preview-icon">📄</span>
          <span class="preview-filename">{{ selectedFileName }}</span>
        </div>
      </aside>

      <div class="result-card">
      <h2>Analysis Result</h2>
      <dl class="meta">
        <dt>ID</dt><dd>{{ result.id }}</dd>
        <dt>Status</dt><dd>{{ result.status }}</dd>
        <dt>Scenario</dt><dd>{{ result.scenario ?? '—' }}</dd>
        <dt>File</dt><dd>{{ result.fileName }}</dd>
        <dt>Analyzed</dt><dd>{{ new Date(result.analyzedAt).toLocaleString() }}</dd>
        <dt>API Duration</dt><dd>{{ result.analysisDurationMs != null ? `${result.analysisDurationMs.toFixed(0)} ms` : '—' }}</dd>
        <dt>Duration</dt><dd>{{ analysisDuration != null ? `${analysisDuration.toFixed(0)} ms` : '—' }}</dd>
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
      </div>
    </template>
  </section>
</template>

<style scoped>
.analyze-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.result-layout {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 768px) {
  .result-layout {
    grid-template-columns: 1fr;
  }
}

.preview-panel {
  background: var(--color-surface, #fff);
  border-radius: var(--radius, 8px);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  padding: 1rem;
  overflow: hidden;
  position: sticky;
  top: 1rem;
  max-height: calc(100vh - 2rem);
  overflow-y: auto;
}

.preview-panel h3 {
  font-size: 1rem;
  margin-bottom: 0.75rem;
}

.preview-image {
  width: 100%;
  height: auto;
  border-radius: 4px;
  display: block;
}

.preview-pdf {
  width: 100%;
  height: 70vh;
  border: none;
  border-radius: 4px;
}

.preview-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 2rem;
  background: #f9f9fb;
  border-radius: 4px;
  color: var(--color-text-secondary, #616161);
}

.preview-icon {
  font-size: 3rem;
}

.preview-filename {
  font-size: 0.85rem;
  word-break: break-all;
  text-align: center;
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

.url-input-group {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  padding: 1rem;
  background: var(--color-surface, #fff);
  border-radius: var(--radius, 8px);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.url-input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid #d0d0d0;
  border-radius: calc(var(--radius, 8px) - 2px);
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.15s;
}

.url-input:focus {
  border-color: var(--color-primary, #0078d4);
}

.url-submit-btn {
  padding: 0.5rem 1.25rem;
  background: var(--color-primary, #0078d4);
  color: #fff;
  border: none;
  border-radius: calc(var(--radius, 8px) - 2px);
  font-size: 0.95rem;
  cursor: pointer;
  white-space: nowrap;
  transition: opacity 0.15s;
}

.url-submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.url-submit-btn:not(:disabled):hover {
  opacity: 0.88;
}

.status-url {
  max-width: 60ch;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
