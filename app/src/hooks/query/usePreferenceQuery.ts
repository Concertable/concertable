import { useQuery } from "@tanstack/react-query";
import preferenceApi from "@/api/preferenceApi";

export function useMyPreferenceQuery() {
  return useQuery({
    queryKey: ["preference", "my"],
    queryFn: preferenceApi.getMyPreference,
  });
}
