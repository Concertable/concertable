import { HeaderType } from "./header-type";

export interface SearchParams {
    searchTerm?: string;
    date?: Date;
    sort?: string;
    latitude?: number;
    longitude?: number;
    radiusKm?: number;
    genreIds?: number[];
}