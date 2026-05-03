import api from "@/lib/axios";
import type { AcceptOutcome, Application, Checkout } from "../types";

const applicationApi = {
  applyToOpportunity: async (
    opportunityId: number,
    paymentMethodId?: string,
  ): Promise<Application> => {
    const { data } = await api.post<Application>(
      `/application/${opportunityId}`,
      paymentMethodId ? { paymentMethodId } : undefined,
    );
    return data;
  },

  applyCheckout: async (opportunityId: number): Promise<Checkout> => {
    const { data } = await api.post<Checkout>(
      `/application/opportunity/${opportunityId}/checkout`,
    );
    return data;
  },

  canApply: async (opportunityId: number): Promise<boolean> => {
    const { data } = await api.get<boolean>(
      `/application/opportunity/${opportunityId}/eligibility`,
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
      paymentMethodId ? { paymentMethodId } : undefined,
    );
    return data;
  },

  canAccept: async (applicationId: number): Promise<boolean> => {
    const { data } = await api.get<boolean>(
      `/application/${applicationId}/eligibility`,
    );
    return data;
  },

  acceptCheckout: async (applicationId: number): Promise<Checkout> => {
    const { data } = await api.post<Checkout>(
      `/application/${applicationId}/checkout`,
    );
    return data;
  },
};

export default applicationApi;
