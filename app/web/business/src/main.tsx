import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";

function App() {
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
            <a data-testid="get-started-artist" className="cta" href={`${import.meta.env.VITE_ARTIST_WEB_URL}/login`}>
              Get started as an artist →
            </a>
          </div>

          <div className="card">
            <h2>For Venues</h2>
            <p>Post opportunities, review applications, run shows and settle with artists.</p>
            <a data-testid="get-started-venue" className="cta" href={`${import.meta.env.VITE_VENUE_WEB_URL}/login`}>
              Get started as a venue →
            </a>
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

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
