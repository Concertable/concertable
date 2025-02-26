import { Injectable } from '@angular/core';
import { Params } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export abstract class ObjectSerializerService<T> {
  abstract serialize(obj: Partial<T>): Params

  abstract deserialize(params: Params): Partial<T>;
}
