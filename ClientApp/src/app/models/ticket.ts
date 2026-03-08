import { Concert } from "./concert";
import { User } from "./user";

export interface Ticket {
    id: number;
    purchaseDate: Date;
    qrCode?: string;
    concert: Concert;
    user: User;
  }
