<script setup lang="ts">
import { ref, computed } from 'vue';
import type { AnalysisResponse } from '../types/analysis';

const props = defineProps<{ result: AnalysisResponse; videoSrc?: string }>();

const videoRef = ref<HTMLVideoElement | null>(null);
const currentTime = ref(0);

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = Math.floor(seconds % 60);
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}

function seekTo(seconds: number): void {
  if (videoRef.value) {
    videoRef.value.currentTime = seconds;
    videoRef.value.play();
  }
}

const activeSegmentIdx = computed(() => {
  if (!props.result.transcript) return -1;
  let active = -1;
  for (let i = 0; i < props.result.transcript.length; i++) {
    if (props.result.transcript[i].startTimeSeconds <= currentTime.value) active = i;
    else break;
  }
  return active;
});
</script>

<template>
  <div class="video-result">
    <div class="result-card video-player-card">
      <h2>Video</h2>
      <video
        ref="videoRef"
        :src="props.videoSrc ?? result.fileName"
        controls
        class="video-player"
        @timeupdate="currentTime = (videoRef?.currentTime ?? 0)"
      />
    </div>

    <div v-if="result.summary" class="result-card summary-card">
      <h2>Summary</h2>
      <p class="summary-text">{{ result.summary }}</p>
    </div>

    <div class="result-card transcript-card">
      <h2>Transcript</h2>
      <template v-if="result.transcript && result.transcript.length">
        <div class="transcript-list">
          <div
            v-for="(seg, idx) in result.transcript"
            :key="idx"
            class="transcript-row"
            :class="{ active: idx === activeSegmentIdx }"
            @click="seekTo(seg.startTimeSeconds)"
          >
            <span class="timestamp">{{ formatTime(seg.startTimeSeconds) }}</span>
            <span class="speaker-chip">{{ seg.speaker }}</span>
            <span class="segment-text">{{ seg.text }}</span>
          </div>
        </div>
      </template>
      <p v-else class="no-transcript">No transcript available.</p>
    </div>
  </div>
</template>

<style scoped>
.video-result {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
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

.summary-text {
  line-height: 1.7;
  color: var(--color-text, #1f1f1f);
  margin: 0;
}

.transcript-list {
  display: flex;
  flex-direction: column;
}

.transcript-row {
  display: grid;
  grid-template-columns: 4rem 8rem 1fr;
  gap: 0.75rem;
  align-items: baseline;
  padding: 0.5rem 0.25rem;
  border-bottom: 1px solid #f0f0f4;
}

.transcript-row:last-child {
  border-bottom: none;
}

.transcript-row:nth-child(even) {
  background: #fafafa;
}

.timestamp {
  font-family: monospace;
  font-size: 0.85rem;
  color: var(--color-text-secondary, #616161);
  white-space: nowrap;
}

.speaker-chip {
  display: inline-block;
  background: #e8f0fe;
  color: #1a73e8;
  border-radius: 12px;
  padding: 0.15rem 0.6rem;
  font-size: 0.8rem;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.segment-text {
  font-size: 0.9rem;
  line-height: 1.5;
  color: var(--color-text, #1f1f1f);
}

.no-transcript {
  font-size: 0.875rem;
  color: var(--color-text-secondary, #616161);
  font-style: italic;
  margin: 0;
}

.transcript-row {
  cursor: pointer;
}

.transcript-row:hover {
  background: #f0f4ff;
}

.transcript-row.active {
  background: #e8f4fd;
  border-left: 3px solid #1a73e8;
  padding-left: calc(0.25rem - 3px);
}

.video-player-card {
  /* inherits .result-card styles */
}

.video-player {
  width: 100%;
  max-height: 480px;
  border-radius: 4px;
  background: #000;
  display: block;
}
</style>
