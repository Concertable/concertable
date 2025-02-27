import { Header } from "./header";

export interface EventHeader extends Header {
    startDate: Date;
    endTime: Date;
}