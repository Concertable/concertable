import { HeaderType } from "./header-type";
import { PaginationParams } from "./pagination-params";

export interface SearchParams extends PaginationParams {
    searchTerm?: string;
    headerType?: HeaderType
    date?: Date;
    sort?: string;
    latitude?: number;
    longitude?: number;
    radiusKm?: number;
    genreIds?: number[];
}