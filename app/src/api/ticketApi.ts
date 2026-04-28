import api from "@/lib/axios";
import type { Ticket } from "@/types/ticket";

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
  getUpcoming: async (): Promise<Ticket[]> => {
    const { data } = await api.get<Ticket[]>("/ticket/upcoming/user");
    return data;
  },
  getHistory: async (): Promise<Ticket[]> => {
    const { data } = await api.get<Ticket[]>("/ticket/history/user");
    return data;
  },
};

export default ticketApi;
