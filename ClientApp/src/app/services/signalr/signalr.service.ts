import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import * as signalR from "@microsoft/signalr";
import { BehaviorSubject, Subject } from 'rxjs';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';
import { Event } from '../../models/event';
import { EventHeader } from '../../models/event-header';
import { TicketPurchase } from '../../models/ticket-purchase';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private paymentHubUrl = environment.paymentHubUrl;
  private eventHubUrl = environment.eventHubUrl; // Add event hub URL

  private paymentHubConnection?: signalR.HubConnection;
  private eventHubConnection?: signalR.HubConnection;

  private eventCreatedSubject = new Subject<ListingApplicationPurchase>();
  private eventPostedSubject = new Subject<EventHeader>();
  private ticketPurchasedSubject = new Subject<TicketPurchase>();

  eventCreated$ = this.eventCreatedSubject.asObservable();
  eventPosted$ = this.eventPostedSubject.asObservable();
  ticketPurchased$ = this.ticketPurchasedSubject.asObservable();

  createHubConnections() {
    this.createPaymentHubConnection();
    this.createEventHubConnection();
  }

  stopHubConnections() {
    this.stopHubConnection(this.paymentHubConnection);
    this.stopHubConnection(this.eventHubConnection);
  }

  public createPaymentHubConnection() {
    this.paymentHubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.paymentHubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    this.paymentHubConnection.start().catch(error => console.log('PaymentHub Connection Error:', error));

    this.paymentHubConnection.on('EventCreated', (response: ListingApplicationPurchase) => 
      this.eventCreatedSubject.next(response)
    );

    this.paymentHubConnection.on('TicketPurchased', (response: TicketPurchase) => 
      this.ticketPurchasedSubject.next(response)
    );
  }

  public createEventHubConnection() {
    this.eventHubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.eventHubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    this.eventHubConnection.start().catch(error => console.log('EventHub Connection Error:', error));

    this.eventHubConnection.on('EventPosted', (event: EventHeader) => {
      this.eventPostedSubject.next(event);
    });
  }

  private stopHubConnection(hubConnection?: signalR.HubConnection) {
    if (hubConnection?.state == signalR.HubConnectionState.Connected) {
      hubConnection.stop().catch(error => console.log('Error stopping hub connection:', error));
    }
  }
}
