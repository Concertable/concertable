export { useAuthStore } from "./store/useAuthStore";
export { useRole } from "./hooks/useRole";
export { useRouteRole } from "./hooks/useRouteRole";
export { userManager, onSigninCallback } from "./config/oidcConfig";
export { requireAuth, requireRole } from "./guards";
export type {
  Role,
  UserRole,
  User,
  VenueManager,
  ArtistManager,
  Customer,
  Admin,
} from "./types";
export { isVenueManager, isArtistManager } from "./types";
