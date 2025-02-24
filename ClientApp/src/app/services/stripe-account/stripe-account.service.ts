import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { AddedBankAccountResponse } from '../../models/added-bank-account-response';

@Injectable({
  providedIn: 'root'
})
export class StripeAccountService {

  private apiUrl = `${environment.apiUrl}/stripeaccount`;
      
  constructor(private http: HttpClient) { }
  
  addBankAccount(token: string): Observable<AddedBankAccountResponse> {
    return this.http.post<AddedBankAccountResponse>(`${this.apiUrl}/add-bank-account?token=${token}`, {});
  }

  isUserVerified(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/verified`, { headers: { 'Content-Type': 'text/plain' } })
  }
  
}
