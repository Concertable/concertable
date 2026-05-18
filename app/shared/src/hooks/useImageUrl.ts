import { useQuery } from "@tanstack/react-query";
import blobApi from "../api/blobApi";

export function useImageUrl(fileName?: string | null) {
  return useQuery({
    queryKey: ["image", fileName],
    queryFn: () =>
      /^(blob:|data:)/.test(fileName!)
        ? Promise.resolve(fileName!)
        : blobApi.download(fileName!),
    enabled: !!fileName,
    staleTime: Infinity,
    meta: { expectedErrors: [404] },
  });
}
