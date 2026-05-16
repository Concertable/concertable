import { useQuery } from "@tanstack/react-query";
import api from "../lib/axiosClient";

async function fetchImageData(fileName: string): Promise<string> {
  const response = await api.get(`/blob/download/${fileName}`, {
    responseType: "arraybuffer",
  });
  const bytes = new Uint8Array(response.data as ArrayBuffer);
  const chunkSize = 8192;
  let binary = "";
  for (let i = 0; i < bytes.length; i += chunkSize)
    binary += String.fromCharCode(...Array.from(bytes.subarray(i, i + chunkSize)));
  const contentType = (response.headers["content-type"] as string | undefined) ?? "image/jpeg";
  return `data:${contentType};base64,${btoa(binary)}`;
}

export function useImageUrl(fileName?: string | null) {
  return useQuery({
    queryKey: ["image", fileName],
    queryFn: () => fetchImageData(fileName!),
    enabled: !!fileName,
    staleTime: Infinity,
  });
}
