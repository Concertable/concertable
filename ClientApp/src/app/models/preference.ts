import { Genre } from "./genre";
import { User } from "./user";

export interface Preference {
    id: number;
    user: User;
    radiusKm: number;
    genres: Genre[];
}