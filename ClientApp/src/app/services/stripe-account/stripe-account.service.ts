import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { SKIP_ERROR_HANDLER } from '../../shared/http/http-context.token';

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
    const context = new HttpContext().set(SKIP_ERROR_HANDLER, true);
    return this.http.get<boolean>(`${this.apiUrl}/verified`, { headers: { 'Content-Type': 'text/plain' }, context })
  }
}
