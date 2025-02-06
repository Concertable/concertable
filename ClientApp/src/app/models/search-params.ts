import { HeaderType } from "./header-type";

export interface SearchParams {
    searchTerm?: string;
    headerType: HeaderType
    location?: string;
    date?: Date;
    sort?: string;
}