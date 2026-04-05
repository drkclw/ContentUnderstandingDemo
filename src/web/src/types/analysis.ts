export type FieldType =
  | 'string'
  | 'number'
  | 'integer'
  | 'boolean'
  | 'date'
  | 'time'
  | 'object'
  | 'array'
  | 'unknown';

export type FieldValue =
  | string
  | number
  | boolean
  | null
  | Record<string, FieldResult>
  | FieldResult[];

export interface FieldResult {
  name: string;
  value: FieldValue;
  confidence: number | null;
  type?: FieldType | null;
}

export interface TranscriptSegment {
  speaker: string;
  text: string;
  startTimeSeconds: number;
  endTimeSeconds: number;
}

export interface AnalysisResponse {
  id: string;
  status: string;
  scenario: string | null;
  fileName: string;
  analyzedAt: string;
  analysisDurationMs: number | null;
  fields: Record<string, FieldResult> | null;
  summary?: string | null;
  transcript?: TranscriptSegment[] | null;
}
