<script setup lang="ts">
import { ref, computed } from 'vue';
import HomeView from './views/HomeView.vue';
import AnalyzeView from './views/AnalyzeView.vue';
import { scenarios } from './config/scenarios';

const activeScenario = ref<string | null>(null);

const activeTitle = computed(() =>
  scenarios.find((s) => s.id === activeScenario.value)?.title,
);

function selectScenario(key: string) {
  activeScenario.value = key;
}

function goHome() {
  activeScenario.value = null;
}
</script>

<template>
  <div class="app">
    <header>
      <div class="header-row">
        <button v-if="activeScenario" class="back-btn" @click="goHome">← Back</button>
        <h1>Azure Content Understanding Demo</h1>
      </div>
      <p v-if="!activeScenario">Upload a document or image to analyze its content using Azure AI.</p>
      <p v-else class="scenario-label">{{ activeTitle }}</p>
    </header>
    <main>
      <HomeView v-if="!activeScenario" @select="selectScenario" />
      <AnalyzeView v-else :scenario="activeScenario" />
    </main>
  </div>
</template>

<style>
:root {
  --color-primary: #0078d4;
  --color-bg: #f5f5f5;
  --color-surface: #ffffff;
  --color-text: #242424;
  --color-text-secondary: #616161;
  --radius: 8px;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
  background: var(--color-bg);
  color: var(--color-text);
}

.app {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

header {
  margin-bottom: 2rem;
}

header h1 {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--color-primary);
}

header p {
  color: var(--color-text-secondary);
  margin-top: 0.5rem;
}

.header-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.back-btn {
  background: none;
  border: 1px solid #d0d0d0;
  border-radius: var(--radius);
  padding: 0.35rem 0.75rem;
  font-size: 0.85rem;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: border-color 0.2s, color 0.2s;
}

.back-btn:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.scenario-label {
  font-weight: 500;
  color: var(--color-text-secondary);
  margin-top: 0.5rem;
}
</style>
