import { UserManager, WebStorageStateStore } from "oidc-client-ts";

export const userManager = new UserManager({
  authority: import.meta.env.VITE_AUTH_AUTHORITY,
  client_id: "concertable-spa",
  redirect_uri: `${window.location.origin}/auth/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: "code",
  scope: "openid profile roles concertable.api offline_access",
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  automaticSilentRenew: true,
});

export const onSigninCallback = () => {
  window.history.replaceState({}, document.title, window.location.pathname);
};
