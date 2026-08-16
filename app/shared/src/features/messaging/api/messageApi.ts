import { apiClient } from "../../../lib/apiClient";
import type { Pagination } from "../../../types/common";
import type { PaginationParams } from "../../../hooks/usePagination";
import type { Message, ReportMessageRequest } from "../types";

const messageApi = {
  getUnreadCount: async (): Promise<number> => {
    const { data } = await apiClient.get<number>("/message/user/unread-count");
    return data;
  },

  getMessages: async (
    params: PaginationParams,
  ): Promise<Pagination<Message>> => {
    const { data } = await apiClient.get<Pagination<Message>>("/message/user", {
      params,
    });
    return data;
  },

  reportMessage: async (
    messageId: number,
    request: ReportMessageRequest,
  ): Promise<void> => {
    await apiClient.post(`/message/${messageId}/report`, request);
  },

  markInboxRead: async (): Promise<number> => {
    const { data } = await apiClient.post<number>("/message/mark-read");
    return data;
  },
};

export default messageApi;
