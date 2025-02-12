import { Component, Input } from '@angular/core';
import { ConfigTextDirective } from '../../../directives/config-text/config-text.directive';

@Component({
  selector: 'app-heading-medium',
  standalone: false,
  
  templateUrl: './heading-medium.component.html',
  styleUrl: './heading-medium.component.scss'
})
export class HeadingMediumComponent extends ConfigTextDirective {
}
