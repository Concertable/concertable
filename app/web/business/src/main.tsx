import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { AuthProvider, useAuth } from "react-oidc-context";
import { User } from "oidc-client-ts";
import { userManager } from "shared/features/auth";
import "./index.css";

function redirectForRole(role: string | undefined) {
  if (role === "VenueManager")
    window.location.replace(import.meta.env.VITE_VENUE_WEB_URL);
  else if (role === "ArtistManager")
    window.location.replace(import.meta.env.VITE_ARTIST_WEB_URL);
}

function App() {
  const auth = useAuth();

  const handleGetStarted = (intendedRole: "ArtistManager" | "VenueManager") => {
    if (auth.isAuthenticated && auth.user) {
      const roleRaw = auth.user.profile.role;
      const role = Array.isArray(roleRaw) ? roleRaw[0] : (roleRaw as string | undefined);
      if (role === intendedRole)
        redirectForRole(role);
      else
        auth.signinRedirect({ extraQueryParams: { prompt: "login" } });
    } else {
      auth.signinRedirect();
    }
  };

  return (
    <>
      <main>
        <section className="hero">
          <h1>Concertable for Business</h1>
          <p className="tagline">Whether you book shows or play them, this is where the work happens.</p>
        </section>

        <section className="paths">
          <div className="card">
            <h2>For Artists</h2>
            <p>List your profile, apply to opportunities at venues, manage bookings and get paid.</p>
            <button className="cta" onClick={() => handleGetStarted("ArtistManager")}>
              Get started as an artist →
            </button>
          </div>

          <div className="card">
            <h2>For Venues</h2>
            <p>Post opportunities, review applications, run shows and settle with artists.</p>
            <button className="cta" onClick={() => handleGetStarted("VenueManager")}>
              Get started as a venue →
            </button>
          </div>
        </section>
      </main>

      <footer>
        <a href={import.meta.env.VITE_CUSTOMER_WEB_URL}>
          Looking to buy tickets? Go to concertable.com
        </a>
      </footer>
    </>
  );
}

const onSigninCallback = (user: User | void) => {
  window.history.replaceState({}, document.title, "/");
  if (!user) return;
  const roleRaw = user.profile.role;
  const role = Array.isArray(roleRaw) ? roleRaw[0] : (roleRaw as string | undefined);
  redirectForRole(role);
};

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AuthProvider userManager={userManager} onSigninCallback={onSigninCallback}>
      <App />
    </AuthProvider>
  </StrictMode>,
);
