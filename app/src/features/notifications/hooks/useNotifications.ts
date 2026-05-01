import { useEffect } from "react";
import { useRouter } from "@tanstack/react-router";
import { toast } from "sonner";
import { useQueryClient } from "@tanstack/react-query";
import { notificationConnection } from "@/lib/signalr";
import type {
  TicketPurchasedPayload,
  MessageReceivedPayload,
  ConcertDraftCreatedPayload,
  ConcertPostedPayload,
  ApplicationAcceptedPayload,
} from "../types";

export function useVenueNotifications() {
  const router = useRouter();

  useEffect(() => {
    console.log(
      "[SignalR] useVenueNotifications mounted, connection state:",
      notificationConnection.state,
    );

    notificationConnection.on(
      "MessageReceived",
      (payload: MessageReceivedPayload) => {
        console.log("[SignalR] MessageReceived:", payload);
      },
    );

    notificationConnection.on(
      "ConcertDraftCreated",
      (payload: ConcertDraftCreatedPayload) => {
        console.log(
          "[SignalR] ConcertDraftCreated received, payload:",
          payload,
          "type:",
          typeof payload,
        );
        void router.navigate({
          to: "/venue/my/concerts/concert/$id",
          params: { id: payload },
        });
      },
    );

    return () => {
      console.log("[SignalR] useVenueNotifications unmounted");
      notificationConnection.off("MessageReceived");
      notificationConnection.off("ConcertDraftCreated");
    };
  }, []);
}

export function useArtistNotifications() {
  const router = useRouter();

  useEffect(() => {
    notificationConnection.on(
      "MessageReceived",
      (payload: MessageReceivedPayload) => {
        console.log("[SignalR] MessageReceived:", payload);
      },
    );

    notificationConnection.on(
      "ApplicationAccepted",
      (payload: ApplicationAcceptedPayload) => {
        console.log("[SignalR] ApplicationAccepted:", payload);
        void router.navigate({
          to: "/artist/my/concerts/concert/$id",
          params: { id: payload },
        });
      },
    );

    return () => {
      notificationConnection.off("MessageReceived");
      notificationConnection.off("ApplicationAccepted");
    };
  }, []);
}

export function useCustomerNotifications() {
  const router = useRouter();
  const queryClient = useQueryClient();

  useEffect(() => {
    notificationConnection.on(
      "TicketPurchased",
      (payload: TicketPurchasedPayload) => {
        void queryClient.invalidateQueries({ queryKey: ["tickets"] });
        const count = payload.ticketIds.length;
        toast.success(
          count > 1
            ? `You purchased ${count} tickets`
            : "You purchased a ticket",
          {
            description: "Click to view your ticket",
            action: {
              label: "View",
              onClick: () =>
                void router.navigate({ to: "/profile/tickets/upcoming" }),
            },
          },
        );
      },
    );

    notificationConnection.on(
      "ConcertPosted",
      (payload: ConcertPostedPayload) => {
        console.log("[SignalR] ConcertPosted:", payload);
      },
    );

    return () => {
      notificationConnection.off("TicketPurchased");
      notificationConnection.off("ConcertPosted");
    };
  }, [router, queryClient]);
}
