import { Genre } from "./genre";

export interface NewListing {
    id?: number;
    startDate: Date;
    endDate: Date;
    pay: number;
    genres: Genre[];
}
  