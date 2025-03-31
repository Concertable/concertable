import { Event } from "./event";
import { User } from "./user";

export interface Ticket {
    id: number;
    purchaseDate: Date; 
    qrCode?: string;
    event: Event;
    user: User;
  }