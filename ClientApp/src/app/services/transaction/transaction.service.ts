import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction } from '../../models/transaction';
import { Pagination } from '../../models/pagination';
import { PaginationParams } from '../../models/pagination-params';
import { environment } from '../../../environments/environment';
import { HttpParamsService } from '../http-params/http-params.service';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  apiUrl = `${environment.apiUrl}/transaction`

  constructor(private http: HttpClient, private httpParamsService: HttpParamsService<PaginationParams>) {}

  getTransactions(pageParams: PaginationParams): Observable<Pagination<Transaction>> {
    const params = this.httpParamsService.serialize(pageParams);
    return this.http.get<Pagination<Transaction>>(this.apiUrl, { params });
  }
}
