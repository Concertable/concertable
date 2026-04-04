import api from "@/lib/axios";
import type { Message } from "@/types/message";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";

export async function getUnreadCount(): Promise<number> {
  const { data } = await api.get<number>("/message/user/unread-count");
  return data;
}

export async function getMessages(
  params: PaginationParams,
): Promise<Pagination<Message>> {
  const { data } = await api.get<Pagination<Message>>("/message/user", {
    params,
  });
  return data;
}

export async function markAsRead(messageIds: number[]): Promise<number> {
  const { data } = await api.post<number>("/message/mark-read", { messageIds });
  return data;
}
