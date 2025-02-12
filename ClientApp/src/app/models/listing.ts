import { Genre } from "./genre";

export interface Listing {
    id? : number;
    startDate : Date;
    endDate: Date;
    pay: number;
    genres: Genre[];
}