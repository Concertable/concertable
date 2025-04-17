import { Genre } from "./genre";
import { Venue } from "./venue";

export interface Listing {
    id? : number;
    startDate : Date;
    endDate: Date;
    pay: number;
    genres: Genre[];
}