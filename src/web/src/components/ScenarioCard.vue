<script setup lang="ts">
import type { ScenarioDefinition } from '../config/scenarios';

const props = defineProps<ScenarioDefinition>();
const emit = defineEmits<{ select: [id: string] }>();
</script>

<template>
  <article class="scenario-card" @click="emit('select', props.id)">
    <div class="card-icon">{{ icon }}</div>
    <h3 class="card-title">{{ title }}</h3>
    <p class="card-description">{{ description }}</p>
    <button class="card-action" @click.stop="emit('select', props.id)">
      Try it →
    </button>
  </article>
</template>

<style scoped>
.scenario-card {
  background: var(--color-surface, #fff);
  border: 1px solid #e0e0e0;
  border-radius: var(--radius, 8px);
  padding: 1.75rem 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  cursor: pointer;
  transition: box-shadow 0.2s, border-color 0.2s, transform 0.15s;
}

.scenario-card:hover {
  border-color: var(--color-primary, #0078d4);
  box-shadow: 0 4px 16px rgba(0, 120, 212, 0.12);
  transform: translateY(-2px);
}

.card-icon {
  font-size: 2.5rem;
  line-height: 1;
}

.card-title {
  font-size: 1.15rem;
  font-weight: 600;
  color: var(--color-text, #242424);
}

.card-description {
  font-size: 0.9rem;
  color: var(--color-text-secondary, #616161);
  line-height: 1.5;
  flex: 1;
}

.card-action {
  align-self: flex-start;
  margin-top: 0.5rem;
  padding: 0.5rem 1.25rem;
  background: var(--color-primary, #0078d4);
  color: #fff;
  border: none;
  border-radius: var(--radius, 8px);
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}

.card-action:hover {
  background: #106ebe;
}
</style>
