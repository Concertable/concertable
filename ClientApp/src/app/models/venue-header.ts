import { Header } from "./header";

export interface VenueHeader extends Header {
    county: string;
    town: string;
}