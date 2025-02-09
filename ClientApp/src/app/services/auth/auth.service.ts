import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, Injectable, signal } from '@angular/core';
import { LoginCredentials } from '../../models/login-credentials';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { LoginResponse } from '../../models/login-response';
import { environment } from '../../../environments/environment';
import { User } from '../../models/user';
import { RegisterCredentials } from '../../models/register-credentials';
import { Router } from '@angular/router';
import { Role } from '../../models/role';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/auth`;
  
  private currentUserSubject = new BehaviorSubject<User | null>(null); 
  currentUser$ = this.currentUserSubject.asObservable(); // Every subscription will recieve the current user whenever it changes

  constructor(private http: HttpClient, private router: Router) { }

  isRole = (role: Role) => this.currentUserSubject.getValue()?.role === role;
  isNotRole = (role: Role) => this.currentUserSubject.getValue()?.role !== role;

  isAuthenticated(): boolean {
    return this.currentUserSubject.getValue() !== null;
  }

  login(credentials: LoginCredentials): Observable<any> {
    let params = new HttpParams().append('useCookies', true);
    return this.http.post<any>(`${environment.apiUrl}/login`, credentials, { params }).pipe(
      tap(() => {
        this.getCurrentUser().subscribe();
        this.router.navigateByUrl('/')
      })
    );
  }

  logout() : Observable<void> {
    console.log(`${this.apiUrl}/logout`);
    return this.http.post<void>(`${this.apiUrl}/logout`, {}).pipe(
      tap(() => {
        this.currentUserSubject.next(null);
        this.router.navigateByUrl('/');
      })
    );
  }

  register(credentials: RegisterCredentials) : Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, credentials).pipe(
      tap(() => {
        this.getCurrentUser().subscribe();
        this.router.navigateByUrl('/login');
      })
    );
  }

  getCurrentUser() : Observable<User> {
    console.log("?");
    return this.http.get<User>(`${this.apiUrl}/current-user`).pipe(
      tap(user => {
        this.currentUserSubject.next(user);
      })
    )
  }

  navigateByRole(role: Role) : void {
    if (role === 'VenueManager')
      this.router.navigateByUrl('/venue');
  }
}
