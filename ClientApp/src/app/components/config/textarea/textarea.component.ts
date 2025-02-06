import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-textarea',
  standalone: false,
  
  templateUrl: './textarea.component.html',
  styleUrl: './textarea.component.scss'
})
export class TextareaComponent {
  @Input() editMode? = false;
  @Input() content = '';
}
