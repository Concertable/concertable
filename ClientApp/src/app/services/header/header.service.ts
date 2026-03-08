import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable, InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SearchParams } from '../../models/search-params';
import { Pagination } from '../../models/pagination';
import { SearchParamsSerializerServiceService } from '../search-params-serializer/search-params-serializer-service.service';

export const HEADER_TYPE = new InjectionToken<string>('HeaderType');

@Injectable()
export class HeaderService {
  private apiUrl = `${environment.apiUrl}/header`;

  constructor(
    private http: HttpClient,
    private serializerService: SearchParamsSerializerServiceService,
    @Inject(HEADER_TYPE) private headerType: string
  ) {}

  get<T>(searchParams: Partial<SearchParams>): Observable<Pagination<T>> {
    const params = this.serializerService.serialize(searchParams);
    return this.http.get<Pagination<T>>(`${this.apiUrl}`, { params });
  }

  getByAmount<T>(amount: number): Observable<T[]> {
    const params = new HttpParams().set('headerType', this.headerType);
    return this.http.get<T[]>(`${this.apiUrl}/amount/${amount}`, { params });
  }

  getPopular<T>(): Observable<T[]> {
    const params = new HttpParams().set('headerType', this.headerType);
    return this.http.get<T[]>(`${this.apiUrl}/popular`, { params });
  }

  getFree<T>(): Observable<T[]> {
    const params = new HttpParams().set('headerType', this.headerType);
    return this.http.get<T[]>(`${this.apiUrl}/free`, { params });
  }
}
