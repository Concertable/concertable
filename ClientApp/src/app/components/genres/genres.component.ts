import { Component, Input } from '@angular/core';
import { Genre } from '../../models/genre';

@Component({
  selector: 'app-genres',
  standalone: false,
  templateUrl: './genres.component.html',
  styleUrl: './genres.component.scss'
})
export class GenresComponent {
  @Input() genres: Genre[] = [];
}
