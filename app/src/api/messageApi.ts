import api from "@/lib/axios";
import type { Message } from "@/types/message";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";

const messageApi = {
  getUnreadCount: async (): Promise<number> => {
    const { data } = await api.get<number>("/message/user/unread-count");
    return data;
  },

  getMessages: async (
    params: PaginationParams,
  ): Promise<Pagination<Message>> => {
    const { data } = await api.get<Pagination<Message>>("/message/user", {
      params,
    });
    return data;
  },

  markAsRead: async (messageIds: number[]): Promise<number> => {
    const { data } = await api.post<number>("/message/mark-read", {
      messageIds,
    });
    return data;
  },
};

export default messageApi;
