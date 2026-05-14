import { useQuery } from "@tanstack/react-query";
import api from "@/lib/axios";

async function fetchImageUrl(fileName: string): Promise<string> {
  const response = await api.get<Blob>(`/blob/download/${fileName}`, {
    responseType: "blob",
  });
  return URL.createObjectURL(response.data);
}

export function useImageUrl(fileName?: string | null) {
  return useQuery({
    queryKey: ["image", fileName],
    queryFn: () => fetchImageUrl(fileName!),
    enabled: !!fileName,
    staleTime: Infinity,
  });
}
