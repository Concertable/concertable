const Config = {
  apiUrl: process.env.EXPO_PUBLIC_API_URL ?? "",
  authAuthority: process.env.EXPO_PUBLIC_AUTH_AUTHORITY ?? "",
  authClientId: "concertable-mobile",
  authScopes: ["openid", "profile", "roles", "concertable.api", "offline_access"],
  stripePublishableKey: process.env.EXPO_PUBLIC_STRIPE_KEY ?? "",
  googleMapsApiKey: process.env.EXPO_PUBLIC_GOOGLE_MAPS_API_KEY ?? "",
};

export default Config;
