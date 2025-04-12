import { Injectable } from '@angular/core';
import { Params } from '@angular/router';
import { SearchParams } from '../../models/search-params';
import { ObjectSerializerService } from '../object-serializer/object-serializer.service';
import { HeaderType } from '../../models/header-type';

@Injectable({
  providedIn: 'root'
})
export class SearchParamsSerializerServiceService extends ObjectSerializerService<SearchParams> {
  defaultHeaderType?: HeaderType;

  deserialize(params: Params): SearchParams {
    return {
      searchTerm: params['searchTerm'],
      headerType: params['headerType'] ?? this.defaultHeaderType, 
      date: params['date'] && !isNaN(Date.parse(params['date'])) ? new Date(params['date']) : undefined,
      sort: params['sort'],
      latitude: params['latitude'] ? Number(params['latitude']) : undefined,
      longitude: params['longitude'] ? Number(params['longitude']) : undefined,
      radiusKm: params['radiusKm'] ? Number(params['radiusKm']) : undefined,
      genreIds: params['genreIds'] ? params['genreIds'].split(',').map(Number) : undefined,
      showHistory: this.toBoolean(params['showHistory']),
      showSold: this.toBoolean(params['showSold'])
    };
  }

  private toBoolean(value: any): boolean | undefined {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return undefined;
  }
}
