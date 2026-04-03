import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class BlobStorageService {

  private apiUrl = `${environment.apiUrl}/blob`;

  constructor(private http: HttpClient) { }

  getUrl(fileName?: string) {
    return `${this.apiUrl}/download/${fileName}`;
  }

  upload(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ url: string }>(`${this.apiUrl}/upload`, formData);
  }

  delete(fileName: string) {
    return this.http.delete(`${this.apiUrl}/${fileName}`);
  }
}
