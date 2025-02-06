import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-image',
  standalone: false,
  
  templateUrl: './image.component.html',
  styleUrl: './image.component.scss'
})
export class ImageComponent {
  @Input() editMode?: boolean;
  @Input() src?: string;
  @Input() alt?: string;
}
