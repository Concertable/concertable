import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import * as applicationApi from "@/api/applicationApi";

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

export function useAcceptApplicationMutation(opportunityId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (applicationId: number) =>
      applicationApi.acceptApplication(applicationId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["applications", "opportunity", opportunityId],
      });
    },
  });
}
