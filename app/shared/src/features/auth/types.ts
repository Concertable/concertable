export type TenantType = "Venue" | "Artist";

export interface Membership {
  tenantId: string;
  legalName: string;
  type: TenantType;
}

export interface User {
  id: string;
  email: string;
  isEmailVerified: boolean;
  latitude?: number;
  longitude?: number;
  county?: string;
  town?: string;
  memberships?: Membership[];
}
