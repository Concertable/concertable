import { Injectable } from '@angular/core';
import { FormDataSerializerService } from './form-data-serializer.service';
import { Venue } from '../models/venue';

@Injectable({
  providedIn: 'root'
})
export class VenueFormDataSerializerService extends FormDataSerializerService<Venue> {
  serializeWithImage(venue: Partial<Venue>, image?: File): FormData {
    const formData = new FormData();
  
    for (const [key, value] of Object.entries(venue)) {

        formData.append(`Venue.${key}`, value as any);
      }

    if(image)
      formData.append('Image', image, image.name); 
  
    return formData;
  }

  deserialize(formData: FormData): Venue {
    return {
      id: parseInt(formData.get('Id') as string, 0),
      type: 'venue',
      name: formData.get('Name') as string,
      about: formData.get('About') as string,
      rating: parseFloat(formData.get('Rating') as string),
      imageUrl: formData.get('ImageUrl') as string,
      county: formData.get('County') as string,
      town: formData.get('Town') as string,
      latitude: parseFloat(formData.get('Latitude') as string),
      longitude: parseFloat(formData.get('Longitude') as string),
      email: formData.get('email') as string,
      approved: formData.get('Approved') === 'true',
    };
  }
}
