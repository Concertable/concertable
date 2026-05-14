export type MessageAction =
  | "ApplicationReceived"
  | "ApplicationAccepted"
  | "ConcertPosted";

export interface MessageSender {
  id: string;
  email: string;
}

export interface Message {
  id: number;
  fromUser: MessageSender;
  action?: MessageAction;
  content: string;
}
