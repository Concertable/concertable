export const DASHBOARD_POLLING = {
  fast: 30_000,
  normal: 60_000,
  static: false as const,
} as const;

export type DashboardPollingTier = keyof typeof DASHBOARD_POLLING;
