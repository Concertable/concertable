import api from "../../../lib/axiosClient";
import type { Application, Checkout } from "../types";

const applicationApi = {
  /* agreedToTerms is asserted at this layer — the UI must gate apply/accept behind the
     click-wrap checkbox; never call these from a surface without it. */
  applyToOpportunity: async (opportunityId: number): Promise<Application> => {
    const { data } = await api.post<Application>(
      `/application/${opportunityId}`,
      { agreedToTerms: true },
    );
    return data;
  },

  applyToOpportunityWithPayment: async (
    opportunityId: number,
    paymentMethodId: string,
  ): Promise<Application> => {
    const { data } = await api.post<Application>(
      `/application/${opportunityId}`,
      { agreedToTerms: true, paymentMethodId },
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
    body?: { paymentMethodId: string },
  ): Promise<void> => {
    await api.post(`/application/${applicationId}/accept`, {
      agreedToTerms: true,
      ...body,
    });
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

  withdrawApplication: async (applicationId: number): Promise<void> => {
    await api.post(`/application/${applicationId}/withdraw`);
  },

  rejectApplication: async (applicationId: number): Promise<void> => {
    await api.post(`/application/${applicationId}/reject`);
  },

  cancelApplication: async (applicationId: number): Promise<void> => {
    await api.post(`/application/${applicationId}/cancel`);
  },

  getPendingForArtist: async (): Promise<Application[]> => {
    const { data } = await api.get<Application[]>(
      `/application/artist/pending`,
    );
    return data;
  },

  getRecentDeniedForArtist: async (): Promise<Application[]> => {
    const { data } = await api.get<Application[]>(
      `/application/artist/recently-denied`,
    );
    return data;
  },
};

export default applicationApi;
