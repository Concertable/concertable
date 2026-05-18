import { redirect } from "@tanstack/react-router";
import venueApi from "@concertable/shared/features/venues/api/venueApi";

export async function requireVenue({ pathname }: { pathname: string }) {
  if (pathname === "/create") return;
  const venue = await venueApi.getMyVenue();
  if (venue === null) throw redirect({ to: "/create" });
}
