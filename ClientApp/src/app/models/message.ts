import { Action } from "./action";

export interface Message {
    id: number;
    fromUserId: number
    content: string;
    read: boolean;
    action?: Action;
}

export type MessageAction = 'application' | 'event'