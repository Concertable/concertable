import type { Genre } from "../../../types/common";
import type { Contract } from "../../contracts/types";
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

export type StripeConnectState = "Complete" | "Incomplete" | "ActionRequired" | "Pending";

export interface StripeConnectStatus {
  state: StripeConnectState;
  href: string;
}

export type ActivityType =
  | "ApplicationReceived"
  | "ApplicationAccepted"
  | "ApplicationDeclined"
  | "ConcertSettled"
  | "ReviewReceived"
  | "TicketSold"
  | "MessageReceived";

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

export interface OpportunitySummary {
  id: number;
  venueId: number;
  venueName: string;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
}

export interface OpportunityWithCounts {
  opportunity: OpportunitySummary;
  applicationCount: number;
  daysUntilDeadline: number;
}

export interface OpportunityCard {
  id: number;
  venueId: number;
  venueName: string;
  venueAvatarUrl?: string;
  county: string;
  town: string;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
  fitScore?: number;
  href: string;
}

export interface MessageThread {
  id: number;
  otherPartyName: string;
  otherPartyAvatarUrl?: string;
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
  reviewerAvatarUrl?: string;
  stars: number;
  excerpt?: string;
  at: string;
  href: string;
}
