const Config = {
  apiUrl: process.env.EXPO_PUBLIC_API_URL ?? "",
  searchApiUrl: process.env.EXPO_PUBLIC_SEARCH_API_URL ?? "",
  customerApiUrl: process.env.EXPO_PUBLIC_CUSTOMER_API_URL ?? "",
  paymentApiUrl: process.env.EXPO_PUBLIC_PAYMENT_API_URL ?? "",
  authAuthority: process.env.EXPO_PUBLIC_AUTH_AUTHORITY ?? "",
  authClientId: process.env.EXPO_PUBLIC_OIDC_CLIENT_ID ?? "",
  authClientIdArtist: process.env.EXPO_PUBLIC_OIDC_CLIENT_ID_ARTIST ?? "",
  authScopes: ["openid", "profile", "roles", "concertable.api", "offline_access"],
  urlScheme: process.env.EXPO_PUBLIC_URL_SCHEME ?? "",
  stripePublishableKey: process.env.EXPO_PUBLIC_STRIPE_KEY ?? "",
  googleMapsApiKey: process.env.EXPO_PUBLIC_GOOGLE_MAPS_API_KEY ?? "",
};

export default Config;
