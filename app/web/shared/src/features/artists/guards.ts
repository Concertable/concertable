import { redirect } from "@tanstack/react-router";
import artistApi from "@concertable/shared/features/artists/api/artistApi";

export async function requireArtist({ pathname }: { pathname: string }) {
  if (pathname === "/create") return;
  const artist = await artistApi.getMyArtist();
  if (artist === null) throw redirect({ to: "/create" });
}
