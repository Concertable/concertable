import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StripeAccountService {

  private apiUrl = `${environment.apiUrl}/stripeaccount`;
      
  constructor(private http: HttpClient) {}

  getOnboardingLink(): Observable<string> {
    return this.http.get(`${this.apiUrl}/onboarding-link`, {
      responseType: 'text' 
    });
  }

  isUserVerified(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/verified`, { headers: { 'Content-Type': 'text/plain' } })
  }
}
