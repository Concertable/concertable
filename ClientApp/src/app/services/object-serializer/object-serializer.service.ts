import { Injectable } from '@angular/core';
import { Params } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export abstract class ObjectSerializerService<T> {
  serialize(obj: Partial<T>): Params {
    return Object.fromEntries(
      Object.entries(obj)
        .filter(([_, value]) => value !== undefined) // Remove undefined values
        .map(([key, value]) => {
          if (value instanceof Date) {
            return [key, value.toISOString().split("T")[0]]; 
          }
          if (Array.isArray(value)) {
            return [key, value.join(",")]; 
          }
          return [key, (value as any).toString()]; 
        })
    )
  }

  private convertValue(value: any): string {
    if (Array.isArray(value)) return value.join(','); 
    if (value instanceof Date) return value.toISOString(); 
    return value.toString();
  }

  abstract deserialize(params: Params): Partial<T>;
}
