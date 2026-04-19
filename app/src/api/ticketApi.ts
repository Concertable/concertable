import api from "@/lib/axios";

interface TicketPurchaseRequest {
  concertId: number;
  quantity: number;
  paymentMethodId?: string;
}

export interface TicketPurchaseResponse {
  requiresAction: boolean;
  clientSecret?: string;
  transactionId?: string;
  ticketIds: string[];
  concertId: number;
  amount: number;
  currency?: string;
  purchaseDate: string;
  userEmail?: string;
}

const ticketApi = {
  purchase: async (
    request: TicketPurchaseRequest,
  ): Promise<TicketPurchaseResponse> => {
    const { data } = await api.post<TicketPurchaseResponse>(
      "/ticket/purchase",
      request,
    );
    return data;
  },
};

export default ticketApi;
