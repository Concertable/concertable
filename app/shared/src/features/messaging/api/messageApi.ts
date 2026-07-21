import { apiClient } from "../../../lib/apiClient";
import type { Pagination } from "../../../types/common";
import type { PaginationParams } from "../../../hooks/usePagination";
import type { Message } from "../types";

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

  markAsRead: async (messageIds: number[]): Promise<number> => {
    const { data } = await apiClient.post<number>("/message/mark-read", {
      messageIds,
    });
    return data;
  },
};

export default messageApi;
