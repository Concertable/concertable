import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import applicationApi from "../api/applicationApi";

export function useApplicationsByOpportunityQuery(opportunityId: number) {
  return useQuery({
    queryKey: ["applications", "opportunity", opportunityId],
    queryFn: () => applicationApi.getApplicationsByOpportunityId(opportunityId),
  });
}

export function useApplicationQuery(applicationId: number) {
  return useQuery({
    queryKey: ["applications", applicationId],
    queryFn: () => applicationApi.getApplicationById(applicationId),
  });
}

export function useCheckoutQuery(applicationId: number) {
  return useQuery({
    queryKey: ["applications", applicationId, "checkout"],
    queryFn: () => applicationApi.checkout(applicationId),
  });
}

export function useAcceptApplicationMutation(opportunityId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      paymentMethodId,
    }: {
      applicationId: number;
      paymentMethodId?: string | null;
    }) => applicationApi.acceptApplication(applicationId, paymentMethodId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["applications", "opportunity", opportunityId],
      });
    },
  });
}
