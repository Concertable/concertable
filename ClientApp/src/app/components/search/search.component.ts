import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-search',
  standalone: false,
  
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent {
  searchTerm: string = '';
  location: string = '';
  date?: Date;
 
  @Input() headerType?: HeaderType;
  @Output() search : EventEmitter<SearchParams>  = new EventEmitter<SearchParams>();

  onSearch() {
    if(this.headerType) {
      const searchParams: SearchParams = {
        searchTerm: this.searchTerm,
        location: this.location,
        date: this.date
      }
      this.search.emit(searchParams);
    }
  }
}
