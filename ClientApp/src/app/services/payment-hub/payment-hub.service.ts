import { Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import * as signalR from "@microsoft/signalr";
import { Event } from '../../models/event';
import { BehaviorSubject } from 'rxjs';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';

@Injectable({
  providedIn: 'root'
})
export class PaymentHubService {
  hubUrl = environment.paymentHubUrl
  private hubConnection?: signalR.HubConnection;
  private listingApplicationResponseSubject = new BehaviorSubject<ListingApplicationPurchase | undefined>(undefined);

  listingApplicationResponse$ = this.listingApplicationResponseSubject.asObservable();

  createHubConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

      this.hubConnection.start()
        .catch(error => console.log(error));

      this.hubConnection.on('ListingApplicationPurchaseResponse', (response: ListingApplicationPurchase) => {
        this.listingApplicationResponseSubject.next(response);
      });
  }

  stopHubConnection() {
    if(this.hubConnection?.state == signalR.HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }
}
