import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { ActivatedRoute } from '@angular/router';
import { CustomerFindComponent } from '../customer-find/customer-find.component';

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
    return this.route.component === CustomerFindComponent;
  }

  onHeaderTypeChange() {
    console.log(this.headerType)
    this.headerTypeChange.emit(this.headerType);
  }
}
