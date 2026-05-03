import { useMutation, useQuery } from "@tanstack/react-query";
import { useRole } from "@/features/auth";
import applicationApi from "../api/applicationApi";

export function useApply(opportunityId: number) {
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
    mutationFn: (paymentMethodId?: string) =>
      applicationApi.applyToOpportunity(opportunityId, paymentMethodId),
  });

  const apply = (paymentMethodId?: string) => applyMutate(paymentMethodId);

  return { apply, isPending, error, canApply };
}
