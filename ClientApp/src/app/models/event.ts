import { CoreEntity } from "./core-entity";

export interface Event extends CoreEntity {
 id: number;
 name: string;
 about: string;
 price: number;
 startDate: Date;
 endDate: Date;
 totalTickets: number;
 availableTickets: number;
 imageUrl: string;
}