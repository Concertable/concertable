export type Role = "Customer" | "ArtistManager" | "VenueManager" | "Admin";
export type UserRole = Exclude<Role, "Admin">;

interface BaseUser {
  id: string;
  email: string;
  role?: Role;
  baseUrl: string;
  isEmailVerified: boolean;
  latitude?: number;
  longitude?: number;
  county?: string;
  town?: string;
}

export interface VenueManager extends BaseUser {
  $type: "venueManager";
  venueId?: number;
}

export interface ArtistManager extends BaseUser {
  $type: "artistManager";
  artistId?: number;
}

export interface Customer extends BaseUser {
  $type: "customer";
}

export interface Admin extends BaseUser {
  $type: "admin";
}

export type User = VenueManager | ArtistManager | Customer | Admin;

export function isVenueManager(user: User): user is VenueManager {
  return user.role === "VenueManager";
}

export function isArtistManager(user: User): user is ArtistManager {
  return user.role === "ArtistManager";
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  role: Exclude<Role, "Admin">;
}

export interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}
