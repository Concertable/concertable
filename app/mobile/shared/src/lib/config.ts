const Config = {
  apiUrl: process.env.EXPO_PUBLIC_API_URL ?? "",
  authAuthority: process.env.EXPO_PUBLIC_AUTH_AUTHORITY ?? "",
  authClientId: process.env.EXPO_PUBLIC_OIDC_CLIENT_ID ?? "",
  authScopes: ["openid", "profile", "roles", "concertable.api", "offline_access"],
  urlScheme: process.env.EXPO_PUBLIC_URL_SCHEME ?? "",
  stripePublishableKey: process.env.EXPO_PUBLIC_STRIPE_KEY ?? "",
  googleMapsApiKey: process.env.EXPO_PUBLIC_GOOGLE_MAPS_API_KEY ?? "",
};

export default Config;
