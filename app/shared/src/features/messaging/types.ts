export type MessageAction =
  | "applicationReceived"
  | "applicationAccepted"
  | "concertPosted";

export type MessageSenderKind = "org" | "member";

const MESSAGE_ACTION_LABELS: Record<MessageAction, string> = {
  applicationReceived: "Application received",
  applicationAccepted: "Application accepted",
  concertPosted: "Concert posted",
};

export function messageActionLabel(action: MessageAction): string {
  return MESSAGE_ACTION_LABELS[action];
}

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
