import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-heading-large',
  standalone: false,
  
  templateUrl: './heading-large.component.html',
  styleUrl: './heading-large.component.scss'
})
export class HeadingLargeComponent {
  @Input() editMode? = false;
  @Input() content?: string;
}
