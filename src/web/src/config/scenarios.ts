export interface ScenarioDefinition {
  id: string;
  title: string;
  description: string;
  icon: string;
  inputType?: 'file' | 'url';
}

export const scenarios: ScenarioDefinition[] = [
  {
    id: 'invoice',
    title: 'Invoice Analysis',
    description: 'Extract key fields from invoices — vendor, dates, line items, totals',
    icon: '🧾',
  },
  {
    id: 'utility-bill',
    title: 'Utility Bill Analysis',
    description: 'Extract account details, charges, usage data, and payment info from utility bills',
    icon: '💡',
  },
  {
    id: 'receipt',
    title: 'Receipt Analysis',
    description: 'Extract merchant info, items, taxes, and totals from purchase receipts',
    icon: '🛒',
  },
  {
    id: 'custom',
    title: 'Custom Analysis',
    description: 'Analyze any document with the default content understanding model',
    icon: '🔍',
  },
  {
    id: 'video',
    title: 'Video Analysis',
    description: 'Analyze video content — extract shots, scenes, and key moments using Azure Content Understanding',
    icon: '🎬',
    inputType: 'url',
  },
];
