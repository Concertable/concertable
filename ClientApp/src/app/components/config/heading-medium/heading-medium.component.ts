import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-heading-medium',
  standalone: false,
  
  templateUrl: './heading-medium.component.html',
  styleUrl: './heading-medium.component.scss'
})
export class HeadingMediumComponent {
  @Input() editMode? = false;
  @Input() content?: string;
  @Input() label?: string;
}
