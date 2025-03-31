import { Action } from "./action";

export interface Message {
    id: number;
    fromUserId: number
    content: string;
    action?: Action;
}

export type MessageAction = 'application' | 'event'