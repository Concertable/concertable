import { useMutation } from "@tanstack/react-query";
import { useRole } from "@/features/auth";
import opportunityApi from "../api/opportunityApi";

export function useApply(opportunityId: number) {
  const role = useRole();

  const canApply = role === "ArtistManager";

  const {
    mutate: apply,
    isPending,
    error,
  } = useMutation({
    mutationFn: () => opportunityApi.applyToOpportunity(opportunityId),
  });

  return { apply, isPending, error, canApply };
}
