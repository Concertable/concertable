import type { ReviewSummary } from "../../reviews/types";

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
  "Complete" | "Incomplete" | "ActionRequired" | "Pending";

export interface StripeConnectStatus {
  state: StripeConnectState;
  href: string;
}

export type ActivityType =
  | "ApplicationReceived"
  | "ApplicationAccepted"
  | "ApplicationDeclined"
  | "ApplicationWithdrawn"
  | "ApplicationCancelled"
  | "ConcertSettled"
  | "ReviewReceived"
  | "TicketSold"
  | "MessageReceived";

export interface ActivityItem {
  id: string;
  type: ActivityType;
  at: string;
  subject: string;
  detail: string | null;
  url: string;
}

export interface MonthlyRevenuePoint {
  month: string;
  grossCents: number;
  netCents: number;
  count: number;
}

export type SettlementDirection = "In" | "Out";

export interface Settlement {
  id: number;
  concertId: number;
  concertName: string;
  at: string;
  amountCents: number;
  counterpartyName: string;
  direction: SettlementDirection;
}

export interface MessagePreview {
  id: number;
  otherPartyName: string;
  otherPartyAvatarUrl: string | null;
  preview: string;
  at: string;
  unread: boolean;
  href: string;
}

export type DashboardApplicationStatus =
  | "Pending"
  | "Accepted"
  | "AwaitingPayment"
  | "Confirmed"
  | "Rejected"
  | "Withdrawn";

export interface ConcertCard {
  id: number;
  name: string;
  bannerUrl: string | null;
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
  reviewerAvatarUrl: string | null;
  stars: number;
  excerpt: string | null;
  at: string;
  href: string;
}
