import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-filter',
  standalone: false,
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.css']
})
export class FilterComponent implements OnInit {
  icon: string = 'tune';

  constructor() { }

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

  getGenres() {

  }
}
