import { useEffect } from "react";
import { notificationConnection } from "@/lib/signalr";
import { useRole } from "@/hooks/useRole";
import type {
  TicketPurchasedPayload,
  MessageReceivedPayload,
  ConcertDraftCreatedPayload,
  ConcertPostedPayload,
} from "@/types/notification";

export function useNotifications() {
  const role = useRole();

  useEffect(() => {
    if (!role) return;

    notificationConnection.on("MessageReceived", (payload: MessageReceivedPayload) => {
      console.log("[SignalR] MessageReceived:", payload);
    });

    if (role === "Customer") {
      notificationConnection.on("TicketPurchased", (payload: TicketPurchasedPayload) => {
        console.log("[SignalR] TicketPurchased:", payload);
      });

      notificationConnection.on("ConcertPosted", (payload: ConcertPostedPayload) => {
        console.log("[SignalR] ConcertPosted:", payload);
      });
    }

    if (role === "VenueManager") {
      notificationConnection.on("ConcertDraftCreated", (payload: ConcertDraftCreatedPayload) => {
        console.log("[SignalR] ConcertDraftCreated:", payload);
      });
    }

    return () => {
      notificationConnection.off("MessageReceived");
      notificationConnection.off("TicketPurchased");
      notificationConnection.off("ConcertPosted");
      notificationConnection.off("ConcertDraftCreated");
    };
  }, [role]);
}
