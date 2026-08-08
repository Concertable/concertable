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

export interface Message {
  id: number;
  counterpartTenantId: string;
  sender: MessageSender;
  action?: MessageAction;
  content: string;
}
