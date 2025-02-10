import { HeaderType } from "./header-type";

export interface SearchParams {
    searchTerm?: string;
    location?: string;
    date?: Date;
    sort?: string;
}