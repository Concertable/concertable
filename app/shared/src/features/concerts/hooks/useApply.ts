import { useMutation, useQuery } from "@tanstack/react-query";
import { useRole } from "../../auth/hooks/useRole";
import applicationApi from "../api/applicationApi";

export function useApply(opportunityId: number, options?: { onSuccess?: () => void }) {
  const role = useRole();
  const isArtistManager = role === "ArtistManager";

  const { data: isEligible } = useQuery({
    queryKey: ["applications", "opportunity", opportunityId, "eligibility"],
    queryFn: () => applicationApi.canApply(opportunityId),
    enabled: isArtistManager,
  });

  const canApply = isArtistManager && isEligible === true;

  const {
    mutate: applyMutate,
    isPending,
    error,
  } = useMutation({
    mutationFn: () => applicationApi.applyToOpportunity(opportunityId),
    onSuccess: () => options?.onSuccess?.(),
  });

  const apply = () => applyMutate();

  return { apply, isPending, error, canApply };
}
