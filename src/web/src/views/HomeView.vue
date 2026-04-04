<script setup lang="ts">
import ScenarioCard from '../components/ScenarioCard.vue';
import { scenarios } from '../config/scenarios';

const emit = defineEmits<{ select: [scenario: string] }>();
</script>

<template>
  <section class="home-view">
    <p class="subtitle">Choose a scenario to get started</p>
    <div class="scenario-grid">
      <ScenarioCard
        v-for="s in scenarios"
        :key="s.id"
        v-bind="s"
        @select="emit('select', $event)"
      />
    </div>
  </section>
</template>

<style scoped>
.home-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.subtitle {
  color: var(--color-text-secondary, #616161);
  font-size: 1rem;
}

.scenario-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.25rem;
}
</style>
