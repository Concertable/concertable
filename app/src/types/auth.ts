export type Role = "Customer" | "ArtistManager" | "VenueManager" | "Admin";

export interface User {
  id: number;
  email: string;
  role?: Role;
  latitude?: number;
  longitude?: number;
  county?: string;
  town?: string;
  baseUrl: string;
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
  userId: string;
  token: string;
  newPassword: string;
}
