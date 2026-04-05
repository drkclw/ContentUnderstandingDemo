import type { AnalysisResponse } from '@/types/analysis';

const BASE_URL = '/api';

export async function analyzeFile(file: File, scenario?: string): Promise<AnalysisResponse> {
  const formData = new FormData();
  formData.append('file', file);

  const url = scenario
    ? `${BASE_URL}/analyze?scenario=${encodeURIComponent(scenario)}`
    : `${BASE_URL}/analyze`;

  const response = await fetch(url, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Analysis failed (${response.status})`);
  }

  return response.json();
}

export async function getResult(id: string): Promise<AnalysisResponse> {
  const response = await fetch(`${BASE_URL}/analyze/${encodeURIComponent(id)}`);

  if (!response.ok) {
    throw new Error(`Failed to retrieve result (${response.status})`);
  }

  return response.json();
}

export async function analyzeUrl(url: string, scenario: string): Promise<AnalysisResponse> {
  const response = await fetch('/api/analyze/url', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ url, scenario }),
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Request failed: ${response.status}`);
  }
  return response.json();
}

export async function checkHealth(): Promise<boolean> {
  try {
    const response = await fetch(`${BASE_URL}/health`);
    return response.ok;
  } catch {
    return false;
  }
}
