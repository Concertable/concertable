import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Observable } from 'rxjs';
import { Header } from '../../models/header';
import { HeaderService } from '../../services/header/header.service';
import { SearchType } from '../../models/search-type';
import { SearchParams } from '../../models/search-params';

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
 
  @Input() searchType?: SearchType;
  @Output() search : EventEmitter<SearchParams>  = new EventEmitter<SearchParams>();

  onSearch() {
    const searchParams: SearchParams = {
      searchTerm: this.searchTerm,
      location: this.location,
      date: this.date
    }
    this.search.emit(searchParams);
  }
}
