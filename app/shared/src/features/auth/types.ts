export type Role = "Customer" | "ArtistManager" | "VenueManager" | "Admin";
export type UserRole = Exclude<Role, "Admin">;

export interface User {
  id: string;
  email: string;
  role: Role;
  isEmailVerified: boolean;
  latitude?: number;
  longitude?: number;
  county?: string;
  town?: string;
}
