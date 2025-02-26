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

  serialize(searchParams: Partial<SearchParams>): Params {
    const params: Params = {};

    if (searchParams.searchTerm) params['searchTerm'] = searchParams.searchTerm;
    if (searchParams.headerType) params['headerType'] = searchParams.headerType;
    if (searchParams.date) params['date'] = searchParams.date.toISOString();
    if (searchParams.sort) params['sort'] = searchParams.sort;
    if (searchParams.latitude !== undefined) params['latitude'] = searchParams.latitude.toString();
    if (searchParams.longitude !== undefined) params['longitude'] = searchParams.longitude.toString();
    if (searchParams.radiusKm !== undefined) params['radiusKm'] = searchParams.radiusKm.toString();
    if (searchParams.genreIds?.length) params['genreIds'] = searchParams.genreIds.join(",");

    return params;
  }

  deserialize(params: Params): Partial<SearchParams> {
    return {
      searchTerm: params['searchTerm'],
      headerType: params['headerType'] ?? this.defaultHeaderType, 
      date: params['date'] && !isNaN(Date.parse(params['date'])) ? new Date(params['date']) : undefined,
      sort: params['sort'],
      latitude: params['latitude'] ? Number(params['latitude']) : undefined,
      longitude: params['longitude'] ? Number(params['longitude']) : undefined,
      radiusKm: params['radiusKm'] ? Number(params['radiusKm']) : undefined,
      genreIds: params['genreIds'] ? params['genreIds'].split(',').map(Number) : undefined
    };
  }
}
