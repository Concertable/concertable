import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import userApi from "../api/userApi";

export function useUserQuery() {
  return useQuery({
    queryKey: ["user"],
    queryFn: userApi.getMe,
  });
}

export function useUpdateLocationMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ latitude, longitude }: { latitude: number; longitude: number }) =>
      userApi.updateLocation(latitude, longitude),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["user"] }),
  });
}
