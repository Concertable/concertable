import { Component, Input } from '@angular/core';
import { ConfigTextDirective } from '../../../directives/config-text/config-text.directive';

@Component({
  selector: 'app-textarea',
  standalone: false,
  
  templateUrl: './textarea.component.html',
  styleUrl: './textarea.component.scss'
})
export class TextareaComponent extends ConfigTextDirective {
}
