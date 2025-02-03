import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, Injectable, signal } from '@angular/core';
import { LoginCredentials } from '../../models/login-credentials';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { LoginResponse } from '../../models/login-response';
import { environment } from '../../../environments/environment';
import { User } from '../../models/user';
import { RegisterRequest } from '../../models/register-request';
import { Router } from '@angular/router';
import { Role } from '../../models/role';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient, private router: Router) { }

  apiUrl = `${environment.apiUrl}/auth`
  currentUser = signal<User | null>(null);

  isRole = (role: string) => this.currentUser()?.role === role;
  isNotRole = (role: string) => this.currentUser()?.role !== role;

  login(credentials: LoginCredentials) : Observable<any> {
    let params = new HttpParams();
    params = params.append('useCookies', true);
    return this.http.post<any>(`${environment.apiUrl}/login`, credentials, { params });
  }

  logout() : Observable<void> {
    console.log(`${this.apiUrl}/logout`);
    return this.http.post<void>(`${this.apiUrl}/logout`, {}).pipe(
      tap(() => this.router.navigateByUrl('/'))
    )
  }

  register(credentials: RegisterRequest) : Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, credentials);
  }

  getCurrentUser() : Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/current-user`).pipe(
      map(user => {
        this.currentUser.set(user);
        return user;
      })
    )
  }

  navigateByRole(role: Role) : void {
    if (role === Role.VenueManager)
      this.router.navigateByUrl('/venue');
  }

  matchesRole(role: Role) : boolean {
    return this.currentUser()?.role === role;
  }
}
