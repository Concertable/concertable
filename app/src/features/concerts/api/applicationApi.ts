import api from "@/lib/axios";
import type { AcceptCheckout, AcceptOutcome, Application } from "../types";

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
    paymentMethodId?: string,
  ): Promise<AcceptOutcome> => {
    const { data } = await api.post<AcceptOutcome>(
      `/application/${applicationId}/accept`,
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

  checkout: async (applicationId: number): Promise<AcceptCheckout | null> => {
    const response = await api.post<AcceptCheckout>(
      `/application/${applicationId}/checkout`,
    );
    return response.status === 204 ? null : response.data;
  },
};

export default applicationApi;
