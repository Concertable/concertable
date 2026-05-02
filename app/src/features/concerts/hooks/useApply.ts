import { useMutation } from "@tanstack/react-query";
import { useRole } from "@/features/auth";
import applicationApi from "../api/applicationApi";

export function useApply(opportunityId: number) {
  const role = useRole();

  const canApply = role === "ArtistManager";

  const {
    mutate: apply,
    isPending,
    error,
  } = useMutation({
    mutationFn: () => applicationApi.applyToOpportunity(opportunityId),
  });

  return { apply, isPending, error, canApply };
}
