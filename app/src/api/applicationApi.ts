import api from "@/lib/axios";
import type {
  AcceptOutcome,
  OpportunityApplication,
} from "@/types/application";

const applicationApi = {
  applyToOpportunity: async (
    opportunityId: number,
  ): Promise<OpportunityApplication> => {
    const { data } = await api.post<OpportunityApplication>(
      `/opportunityapplication/${opportunityId}`,
    );
    return data;
  },

  getApplicationsByOpportunityId: async (
    opportunityId: number,
  ): Promise<OpportunityApplication[]> => {
    const { data } = await api.get<OpportunityApplication[]>(
      `/opportunityapplication/opportunity/${opportunityId}`,
    );
    return data;
  },

  getApplicationById: async (
    applicationId: number,
  ): Promise<OpportunityApplication> => {
    const { data } = await api.get<OpportunityApplication>(
      `/opportunityapplication/${applicationId}`,
    );
    return data;
  },

  acceptApplication: async (
    applicationId: number,
    paymentMethodId?: string | null,
  ): Promise<AcceptOutcome> => {
    const { data } = await api.post<AcceptOutcome>(
      `/opportunityapplication/accept/${applicationId}`,
      { paymentMethodId },
    );
    return data;
  },

  canAcceptApplication: async (applicationId: number): Promise<boolean> => {
    const { data } = await api.get<boolean>(
      `/opportunityapplication/can-accept/${applicationId}`,
    );
    return data;
  },
};

export default applicationApi;
