import { Header } from "./header";

export interface ConcertHeader extends Header {
    startDate: Date;
    endDate: Date;
    datePosted?: Date
}
