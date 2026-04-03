import { Injectable } from '@angular/core';
import { FormDataSerializerService } from './form-data-serializer/form-data-serializer.service';
import { Artist } from '../models/artist';

@Injectable({
  providedIn: 'root'
})
export class ArtistFormDataSerializerService extends FormDataSerializerService<Artist> {
  
  serializeWithImage(artist: Partial<Artist>, image?: File): FormData {
    const formData = new FormData();

    for (const [key, value] of Object.entries(artist)) {
      if (key === 'genres' && Array.isArray(value)) {
        value.forEach((genre, index) => {
          formData.append(`Artist.genres[${index}].id`, genre.id.toString());
          formData.append(`Artist.genres[${index}].name`, genre.name);
        });      
      } else if (value !== undefined && value !== null) {
        formData.append(`Artist.${key}`, value as any);
      }
    }

    if (image) {
      formData.append('Image', image, image.name);
    }

    return formData;
  }
}
