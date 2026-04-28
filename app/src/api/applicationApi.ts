import api from "@/lib/axios";
import type { AcceptOutcome, Application } from "@/types/application";
import type { AcceptCheckout } from "@/types/acceptCheckout";

const applicationApi = {
  applyToOpportunity: async (opportunityId: number): Promise<Application> => {
    const { data } = await api.post<Application>(
      `/application/${opportunityId}`,
    );
    return data;
  },

  getApplicationsByOpportunityId: async (
    opportunityId: number,
  ): Promise<Application[]> => {
    const { data } = await api.get<Application[]>(
      `/application/opportunity/${opportunityId}`,
    );
    return data;
  },

  getApplicationById: async (applicationId: number): Promise<Application> => {
    const { data } = await api.get<Application>(
      `/application/${applicationId}`,
    );
    return data;
  },

  acceptApplication: async (
    applicationId: number,
    paymentMethodId?: string | null,
  ): Promise<AcceptOutcome> => {
    const { data } = await api.post<AcceptOutcome>(
      `/application/accept/${applicationId}`,
      { paymentMethodId },
    );
    return data;
  },

  canAcceptApplication: async (applicationId: number): Promise<boolean> => {
    const { data } = await api.get<boolean>(
      `/application/can-accept/${applicationId}`,
    );
    return data;
  },

  checkout: async (applicationId: number): Promise<AcceptCheckout> => {
    const { data } = await api.post<AcceptCheckout>(
      `/application/${applicationId}/checkout`,
    );
    return data;
  },
};

export default applicationApi;
