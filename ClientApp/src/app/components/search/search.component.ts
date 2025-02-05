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
  searchParams?: SearchParams;
  @Input() searchType?: SearchType;
  @Output() search : EventEmitter<SearchParams>  = new EventEmitter<SearchParams>();

  onSearch() {
    this.search.emit(this.searchParams);
  }
}
