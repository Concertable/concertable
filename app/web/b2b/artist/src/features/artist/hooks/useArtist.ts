import { useArtist as useArtistShared } from "@concertable/shared/features/artists";
import { toast } from "sonner";

export function useArtist() {
  return useArtistShared({
    onSuccess: () => toast.success("Artist saved!"),
  });
}
