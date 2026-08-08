export interface User {
  id: string;
  email: string;
  isEmailVerified: boolean;
  latitude?: number;
  longitude?: number;
  county?: string;
  town?: string;
}
