import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { ActivatedRoute } from '@angular/router';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { SearchParams } from '../../models/search-params';
import { CustomerFindComponent } from '../customer-find/customer-find.component';

@Component({
  selector: 'app-filter',
  standalone: false,
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.css']
})
export class FilterComponent implements OnInit {
  @Input() headerType?: HeaderType;
  @Input() searchParams!: SearchParams;
  @Output() headerTypeChange = new EventEmitter<HeaderType | undefined>();
  @Output() searchParamsChange = new EventEmitter<SearchParams>();

  icon: string = 'tune';
  headerTypes: HeaderType[] = ['venue', 'artist', 'event'];
  genres: Genre[] = [];
  selectedGenre?: Genre;
  selectedGenres: Genre[] = [];

  constructor(private route: ActivatedRoute, private genreService: GenreService) {}

  ngOnInit(): void {
    this.getGenres();
  }

  getGenres() {
    this.genreService.getAll().subscribe(g => {
      this.genres = g;
      this.loadSelectedGenres();
    });
  }

  loadSelectedGenres() {
    console.log("genres",this.genres);
    console.log("ids",this.searchParams.genreIds);
    this.searchParams.genreIds?.forEach(id => {
      const genre = this.genres.find(g => g.id === id);
      if (genre) {
        this.selectedGenres.push(genre);
      }
    });

    console.log("selectedgenres",this.selectedGenres);
  }
  

  private emitChanges() {
    const genreIds = this.selectedGenres.map(g => g.id);
    this.searchParams.genreIds = genreIds;
    this.searchParamsChange.emit(this.searchParams);
  }

  isCustomerRoute(): boolean {
    return this.route.component === CustomerFindComponent;
  }

  onHeaderTypeChange() {
    this.headerTypeChange.emit(this.headerType);
  }

  onGenreChange() {
    console.log(this.selectedGenre?.name);
  }

  addGenre() {
    if (this.selectedGenre && !this.selectedGenres.includes(this.selectedGenre)) {
      this.selectedGenres.push(this.selectedGenre);
      this.emitChanges();
    }
  }

  removeGenre(genre: Genre) {
    this.selectedGenres = this.selectedGenres.filter(g => g !== genre);
    this.emitChanges();
  }
}
