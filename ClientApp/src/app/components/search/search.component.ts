import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-search',
  standalone: false,
  
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent implements OnInit{
  ngOnInit(): void {
    console.log("current",this.searchParams);
  }
  @Input() searchParams!: Partial<SearchParams>;

  @Output() search : EventEmitter<void>  = new EventEmitter<void>();
  @Output() searchParamsChange = new EventEmitter<Partial<SearchParams>>();

  onDateChange(date: Date) {
    this.searchParams.date = date;
  }

  onLatLongChange(latLong: google.maps.LatLngLiteral | undefined) {
    this.searchParams.latitude = latLong?.lat;
    this.searchParams.longitude = latLong?.lng;
  }

  onSearch() {
    console.log("emit",this.searchParams.headerType);
    if(this.searchParams.headerType) {
      this.searchParamsChange.emit(this.searchParams);
      this.search.emit();
    }
  }
}
