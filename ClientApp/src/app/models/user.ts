import { Role } from "./role";

export interface User {
    id: number;
    email: string;
    role: Role;
    latitude: number;
    longitude: number;
    county: string;
    town: string;
    baseUrl: string
}

export interface Admin extends User {
    role: "Admin";
    baseUrl: "/admin";
}

export interface Customer extends User {
    role: "Customer";
    baseUrl: "/user";
}

export interface VenueManager extends User {
    role: "VenueManager";
    baseUrl: "/venue-manager";
}

export interface ArtistManager extends User {
    role: "ArtistManager";
    baseUrl: "/artist-manager";
}