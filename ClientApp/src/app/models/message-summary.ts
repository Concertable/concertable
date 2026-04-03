import { Message } from "./message";
import { Pagination } from "./pagination";

export interface MessageSummary {
    messages: Pagination<Message>
    unreadCount: number;
}