import type { ReviewSummary } from "../reviews/types";

export interface ProfileHealthItem {
  id: string;
  label: string;
  href: string;
  done: boolean;
}

export interface ProfileHealth {
  completeness: number;
  items: ProfileHealthItem[];
}

export type StripeConnectState =
  | "complete"
  | "incomplete"
  | "actionRequired"
  | "pending";

export interface StripeConnectStatus {
  state: StripeConnectState;
  href: string;
}

export type ActivityType =
  | "applicationReceived"
  | "applicationAccepted"
  | "applicationDeclined"
  | "applicationWithdrawn"
  | "applicationCancelled"
  | "concertSettled"
  | "reviewReceived"
  | "ticketSold"
  | "messageReceived";

export interface ActivityItem {
  id: string;
  type: ActivityType;
  at: string;
  subject: string;
  detail?: string;
  url: string;
}

export interface MonthlyRevenuePoint {
  month: string;
  grossCents: number;
  netCents: number;
  count: number;
}

export type SettlementDirection = "in" | "out";

export interface Settlement {
  id: number;
  concertId: number;
  concertName: string;
  at: string;
  amountCents: number;
  counterpartyName: string;
  direction: SettlementDirection;
}

export type DashboardApplicationStatus =
  | "pending"
  | "accepted"
  | "awaitingPayment"
  | "confirmed"
  | "rejected"
  | "withdrawn";

export interface ConcertCard {
  id: number;
  name: string;
  bannerUrl?: string;
  startDate: string;
  endDate: string;
  counterpartyName: string;
  ticketsSold: number;
  ticketsTotal: number;
  href: string;
}

export type { ReviewSummary };

export interface ReviewExcerpt {
  id: number;
  reviewerName: string;
  stars: number;
  excerpt?: string;
  at: string;
  href: string;
}
