import { Injectable } from '@angular/core';
import { ISerializer } from '../shared/interfaces/serializer/iserializer';

@Injectable({
  providedIn: 'root'
})
export abstract class FormDataSerializerService<T> implements ISerializer<T> {
  protected capitalizeFirstChar(string: string): string {
    return string.charAt(0).toUpperCase() + string.slice(1);
  }

  serialize(obj: Partial<T>): FormData {
    const formData = new FormData();
  
    for (const [key, value] of Object.entries(obj)) {
      if (value !== undefined) {
        formData.append(this.capitalizeFirstChar(key), value as any);
      }
    }
  
    return formData;
  }

  abstract deserialize(formData: FormData): T
}
