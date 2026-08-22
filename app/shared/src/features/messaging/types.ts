import type { ActionLink } from "../../types/common";
import type { ReportCategory } from "./schemas/reportMessageRequestSchema";

export type {
  ReportCategory,
  ReportMessageRequest,
} from "./schemas/reportMessageRequestSchema";

export const REPORT_CATEGORY_LABELS: Record<ReportCategory, string> = {
  illegalContent: "Illegal content",
  harassment: "Harassment or abuse",
  fraud: "Fraud or scam",
  spam: "Spam",
  other: "Something else",
};

export type MessageAction =
  | "applicationReceived"
  | "applicationAccepted"
  | "concertPosted";

export type MessageSenderKind = "org" | "member";

export const MESSAGE_ACTION_LABELS: Record<MessageAction, string> = {
  applicationReceived: "Application received",
  applicationAccepted: "Application accepted",
  concertPosted: "Concert posted",
};

export interface MessageSender {
  kind: MessageSenderKind;
  displayName: string;
  county: string | null;
  town: string | null;
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
