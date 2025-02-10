import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-filter',
  standalone: false,
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.css']
})
export class FilterComponent {
  @Input() headerType?: HeaderType;
  @Output() headerTypeChange: EventEmitter<HeaderType | undefined> = new EventEmitter<HeaderType | undefined>();

  icon: string = 'tune';
  headerTypes: HeaderType[] = ['venue', 'artist', 'event'];

  constructor(private route: ActivatedRoute) { }

  getGenres() {

  }

  isCustomerRoute(): boolean {
    console.log(this.route.snapshot);
    return this.route.snapshot.url[0].path === 'find';
  }

  onHeaderTypeChange() {
    console.log(this.headerType)
    this.headerTypeChange.emit(this.headerType);
  }
}
