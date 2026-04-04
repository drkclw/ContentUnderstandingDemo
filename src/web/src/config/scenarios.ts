export interface ScenarioDefinition {
  id: string;
  title: string;
  description: string;
  icon: string;
}

export const scenarios: ScenarioDefinition[] = [
  {
    id: 'invoice',
    title: 'Invoice Analysis',
    description: 'Extract key fields from invoices — vendor, dates, line items, totals',
    icon: '🧾',
  },
];
