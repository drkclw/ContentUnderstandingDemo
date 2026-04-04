<script setup lang="ts">
import { ref, computed } from 'vue';
import type { FieldResult, FieldValue } from '../types/analysis';

const props = withDefaults(defineProps<{
  field: FieldResult;
  fieldKey: string;
  depth?: number;
}>(), { depth: 0 });

const indentStyle = computed(() => ({
  paddingLeft: `${0.75 + props.depth * 1.5}rem`,
}));

function formatValue(value: FieldValue): string {
  if (value == null) return '—';
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  return String(value);
}

function formatConfidence(confidence: number | null): string {
  return confidence != null ? (confidence * 100).toFixed(1) + '%' : '—';
}

function isObject(field: FieldResult): boolean {
  return field.type === 'object'
    && typeof field.value === 'object'
    && field.value !== null
    && !Array.isArray(field.value);
}

function isArray(field: FieldResult): boolean {
  return field.type === 'array' && Array.isArray(field.value);
}

function objectEntries(value: FieldValue): [string, FieldResult][] {
  if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
    return Object.entries(value as Record<string, FieldResult>);
  }
  return [];
}

function arrayItems(value: FieldValue): FieldResult[] {
  return Array.isArray(value) ? value : [];
}

function arrayHasObjects(field: FieldResult): boolean {
  const items = arrayItems(field.value);
  return items.length > 0 && items.some((i) => isObject(i));
}

const expanded = ref(!arrayHasObjects(props.field));
</script>

<template>
  <!-- Simple value -->
  <tr v-if="!isObject(field) && !isArray(field)">
    <td :style="indentStyle">{{ field.name }}</td>
    <td>{{ formatValue(field.value) }}</td>
    <td>{{ formatConfidence(field.confidence) }}</td>
  </tr>

  <!-- Object value -->
  <template v-else-if="isObject(field)">
    <tr class="field-row-group-header">
      <td :style="indentStyle">{{ field.name }}</td>
      <td class="field-row-type-badge">object</td>
      <td>{{ formatConfidence(field.confidence) }}</td>
    </tr>
    <FieldRow
      v-for="([childKey, child]) in objectEntries(field.value)"
      :key="`${fieldKey}.${childKey}`"
      :field="child"
      :field-key="`${fieldKey}.${childKey}`"
      :depth="depth + 1"
    />
  </template>

  <!-- Array value -->
  <template v-else-if="isArray(field)">
    <tr class="field-row-group-header field-row-collapsible" @click="expanded = !expanded">
      <td :style="indentStyle">
        <span class="field-row-chevron" :class="{ 'field-row-chevron--open': expanded }">▶</span>
        {{ field.name }}
      </td>
      <td class="field-row-type-badge">array ({{ arrayItems(field.value).length }} items)</td>
      <td>{{ formatConfidence(field.confidence) }}</td>
    </tr>
    <template v-if="expanded">
      <template v-for="(item, idx) in arrayItems(field.value)" :key="`${fieldKey}[${idx}]`">
        <!-- Array item is an object -->
        <template v-if="isObject(item)">
          <tr class="field-row-item-header">
            <td :style="{ paddingLeft: `${0.75 + (depth + 1) * 1.5}rem` }">
              {{ item.name || `Item ${idx + 1}` }}
            </td>
            <td class="field-row-type-badge">object</td>
            <td>{{ formatConfidence(item.confidence) }}</td>
          </tr>
          <FieldRow
            v-for="([childKey, child]) in objectEntries(item.value)"
            :key="`${fieldKey}[${idx}].${childKey}`"
            :field="child"
            :field-key="`${fieldKey}[${idx}].${childKey}`"
            :depth="depth + 2"
          />
        </template>
        <!-- Array item is simple -->
        <tr v-else>
          <td :style="{ paddingLeft: `${0.75 + (depth + 1) * 1.5}rem` }">
            {{ item.name || `#${idx + 1}` }}
          </td>
          <td>{{ formatValue(item.value) }}</td>
          <td>{{ formatConfidence(item.confidence) }}</td>
        </tr>
      </template>
    </template>
  </template>
</template>

<style scoped>
.field-row-group-header td {
  font-weight: 600;
  background: #f9f9fb;
}

.field-row-item-header td {
  font-weight: 600;
  background: #f4f4f8;
}

.field-row-collapsible {
  cursor: pointer;
  user-select: none;
}

.field-row-chevron {
  display: inline-block;
  font-size: 0.7rem;
  margin-right: 0.4rem;
  transition: transform 0.2s ease;
}

.field-row-chevron--open {
  transform: rotate(90deg);
}

.field-row-type-badge {
  font-size: 0.8rem;
  color: var(--color-text-secondary, #616161);
  font-style: italic;
}
</style>
