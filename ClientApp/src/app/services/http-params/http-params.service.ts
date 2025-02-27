import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class HttpParamsService<T> {

  constructor() { }

  serialize(searchParams: Partial<T>): HttpParams {
    return new HttpParams({
      fromObject: Object.fromEntries(
        Object.entries(searchParams)
          .filter(([_, value]) => value !== null && value !== undefined) 
          .map(([key, value]) => [
            key,
            this.convertValue(value) 
          ])
      )
    });
  }

  private convertValue(value: any): string {
    if (Array.isArray(value)) return value.join(','); 
    if (value instanceof Date) return value.toISOString(); 
    return value.toString();
  }
}
