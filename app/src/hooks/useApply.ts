import { useMutation } from "@tanstack/react-query";
import { applyToOpportunity } from "@/api/applicationApi";
import { useRole } from "@/hooks/useRole";

export function useApply(opportunityId: number) {
  const role = useRole();

  const canApply = role === "ArtistManager";

  const {
    mutate: apply,
    isPending,
    error,
  } = useMutation({
    mutationFn: () => applyToOpportunity(opportunityId),
  });

  return { apply, isPending, error, canApply };
}
