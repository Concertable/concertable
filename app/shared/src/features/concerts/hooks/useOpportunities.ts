import { useQueryClient } from "@tanstack/react-query";
import opportunityApi from "../api/opportunityApi";
import { useOpportunitiesStore } from "../store/useOpportunitiesStore";
import {
  opportunitiesQueryKey,
  useAllOpportunitiesQuery,
} from "./useOpportunitiesQuery";

export function useOpportunities(venueId: number) {
  const enabled = venueId > 0;
  const { isLoading, isError } = useAllOpportunitiesQuery(venueId, enabled);
  const queryClient = useQueryClient();
  const queryKey = opportunitiesQueryKey(venueId);

  const opportunities = useOpportunitiesStore((s) => s.opportunities);
  const drafts = useOpportunitiesStore((s) => s.drafts);
  const isDirty = useOpportunitiesStore((s) => s.isDirty);
  const hydrate = useOpportunitiesStore((s) => s.hydrate);
  const reset = useOpportunitiesStore((s) => s.reset);
  const addDraft = useOpportunitiesStore((s) => s.addDraft);

  const removeOpportunity = useOpportunitiesStore((s) => s.removeOpportunity);
  const setOpportunityDates = useOpportunitiesStore((s) => s.setOpportunityDates);
  const setOpportunityContractType = useOpportunitiesStore((s) => s.setOpportunityContractType);
  const setOpportunityContract = useOpportunitiesStore((s) => s.setOpportunityContract);
  const setOpportunityPaymentMethod = useOpportunitiesStore((s) => s.setOpportunityPaymentMethod);
  const toggleOpportunityGenre = useOpportunitiesStore((s) => s.toggleOpportunityGenre);

  const removeDraft = useOpportunitiesStore((s) => s.removeDraft);
  const setDraftDates = useOpportunitiesStore((s) => s.setDraftDates);
  const setDraftContractType = useOpportunitiesStore((s) => s.setDraftContractType);
  const setDraftContract = useOpportunitiesStore((s) => s.setDraftContract);
  const setDraftPaymentMethod = useOpportunitiesStore((s) => s.setDraftPaymentMethod);
  const toggleDraftGenre = useOpportunitiesStore((s) => s.toggleDraftGenre);

  return {
    opportunities,
    drafts,
    isLoading,
    isError,
    isDirty,
    hydrate,
    reset,
    addDraft,
    opportunityActions: {
      remove: removeOpportunity,
      setDates: setOpportunityDates,
      setContractType: setOpportunityContractType,
      setContract: setOpportunityContract,
      setPaymentMethod: setOpportunityPaymentMethod,
      toggleGenre: toggleOpportunityGenre,
    },
    draftActions: {
      remove: removeDraft,
      setDates: setDraftDates,
      setContractType: setDraftContractType,
      setContract: setDraftContract,
      setPaymentMethod: setDraftPaymentMethod,
      toggleGenre: toggleDraftGenre,
    },
    save: async () => {
      const updated = await opportunityApi.update(venueId, [...opportunities, ...drafts]);
      hydrate(updated);
      await queryClient.invalidateQueries({ queryKey });
    },
  };
}
