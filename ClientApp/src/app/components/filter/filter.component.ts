import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { ActivatedRoute } from '@angular/router';
import { CustomerFindComponent } from '../customer-find/customer-find.component';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';

@Component({
  selector: 'app-filter',
  standalone: false,
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.css']
})
export class FilterComponent implements OnInit {
  @Input() headerType?: HeaderType;
  @Output() headerTypeChange: EventEmitter<HeaderType | undefined> = new EventEmitter<HeaderType | undefined>();

  icon: string = 'tune';
  headerTypes: HeaderType[] = ['venue', 'artist', 'event'];
  genres: Genre[] = [];
  selectedGenre?: Genre;

  constructor(private route: ActivatedRoute, private genreService: GenreService) { }

  ngOnInit(): void {
      this.getGenres();
  }

  getGenres() {
    this.genreService.getAll().subscribe(g => this.genres = g); 
  }

  isCustomerRoute(): boolean {
    return this.route.component === CustomerFindComponent;
  }

  onHeaderTypeChange() {
    console.log(this.headerType)
    this.headerTypeChange.emit(this.headerType);
  }

  onGenreChange() {
    console.log(this.selectedGenre?.name);
  }
}
