import type { NavigatorScreenParams } from "@react-navigation/native";

export type AuthStackParamList = {
  Login: undefined;
};

export type ConcertNavParamList = {
  ConcertDetail: { concertId: number };
  TicketCheckout: { concertId: number };
  CheckoutSuccess: { ticketCount?: number } | undefined;
  ArtistDetail: { artistId: number };
  VenueDetail: { venueId: number };
};

export type HomeStackParamList = {
  HomeMain: undefined;
} & ConcertNavParamList;

export type SearchStackParamList = {
  SearchMain: undefined;
} & ConcertNavParamList;

export type TicketsStackParamList = {
  TicketsMain: undefined;
  TicketDetail: { ticketId: string };
};

export type ProfileStackParamList = {
  ProfileMain: undefined;
  EditProfile: undefined;
  Location: undefined;
  Preferences: undefined;
};

export type CustomerTabParamList = {
  HomeTab: NavigatorScreenParams<HomeStackParamList>;
  SearchTab: NavigatorScreenParams<SearchStackParamList>;
  TicketsTab: NavigatorScreenParams<TicketsStackParamList>;
  Messages: undefined;
  ProfileTab: NavigatorScreenParams<ProfileStackParamList>;
};

export type ArtistTabParamList = {
  Home: undefined;
  Search: undefined;
  Applications: undefined;
  Messages: undefined;
  Profile: undefined;
};

export type VenueTabParamList = {
  Home: undefined;
  Concerts: undefined;
  Messages: undefined;
  Profile: undefined;
};
