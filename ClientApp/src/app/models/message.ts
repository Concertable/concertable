export interface Message {
    id: number;
    fromUserId: number
    content: string;
    action?: MessageAction;
    actionId?: number;
}

export type MessageAction = 'application' | 'event'