import { useMutation } from "@tanstack/react-query";
import { useRole } from "@/features/auth";
import applicationApi from "../api/applicationApi";

export function useApply(opportunityId: number) {
  const role = useRole();

  const canApply = role === "ArtistManager";

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
