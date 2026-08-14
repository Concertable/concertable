import type { ActionLink } from "../../types/common";

export type MessageAction =
  | "ApplicationReceived"
  | "ApplicationAccepted"
  | "ConcertPosted";

export type MessageSenderKind = "Org" | "Member";

export interface MessageSender {
  kind: MessageSenderKind;
  displayName: string;
  county: string | null;
  town: string | null;
}

export type ReportCategory =
  | "IllegalContent"
  | "Harassment"
  | "Fraud"
  | "Spam"
  | "Other";

export interface ReportMessageRequest {
  category: ReportCategory;
  details?: string;
}

export interface MessageActions {
  report?: ActionLink;
}

export interface Message {
  id: number;
  counterpartTenantId: string;
  sender: MessageSender;
  action?: MessageAction;
  content: string;
  actions?: MessageActions;
}
