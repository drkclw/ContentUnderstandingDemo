<script setup lang="ts">
import { ref } from 'vue';

const emit = defineEmits<{
  selected: [file: File];
}>();

const isDragging = ref(false);
const fileInput = ref<HTMLInputElement>();

function onDrop(event: DragEvent) {
  isDragging.value = false;
  const files = event.dataTransfer?.files;
  if (files?.length) {
    emit('selected', files[0]);
  }
}

function onFileChange(event: Event) {
  const target = event.target as HTMLInputElement;
  if (target.files?.length) {
    emit('selected', target.files[0]);
  }
}

function openFilePicker() {
  fileInput.value?.click();
}
</script>

<template>
  <div
    class="upload-zone"
    :class="{ dragging: isDragging }"
    @dragover.prevent="isDragging = true"
    @dragleave="isDragging = false"
    @drop.prevent="onDrop"
    @click="openFilePicker"
  >
    <input
      ref="fileInput"
      type="file"
      accept=".pdf,.png,.jpg,.jpeg,.tiff,.bmp,.docx"
      hidden
      @change="onFileChange"
    />
    <p class="upload-icon">📄</p>
    <p>Drop a file here or click to browse</p>
    <p class="hint">PDF, images, or documents up to 10 MB</p>
  </div>
</template>

<style scoped>
.upload-zone {
  border: 2px dashed #ccc;
  border-radius: var(--radius, 8px);
  padding: 3rem 2rem;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s, background 0.2s;
}

.upload-zone:hover,
.upload-zone.dragging {
  border-color: var(--color-primary, #0078d4);
  background: rgba(0, 120, 212, 0.04);
}

.upload-icon {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.hint {
  color: var(--color-text-secondary, #616161);
  font-size: 0.875rem;
  margin-top: 0.25rem;
}
</style>
