import { Component, Input } from '@angular/core';
import { ConfigTextDirective } from '../../../directives/config-text/config-text.directive';
import { InputDirective } from '../../../directives/input.directive';

@Component({
  selector: 'app-textarea',
  standalone: false,
  
  templateUrl: './textarea.component.html',
  styleUrl: './textarea.component.scss'
})
export class TextareaComponent extends InputDirective {
}
