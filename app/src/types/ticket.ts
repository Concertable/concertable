export interface TicketConcert {
  id: number;
  name: string;
  price: number;
  startDate: string;
  endDate: string;
  venueName: string;
  artistName: string;
}

export interface Ticket {
  id: string;
  purchaseDate: string;
  qrCode: string;
  userId: string;
  userEmail: string;
  concert: TicketConcert;
}
