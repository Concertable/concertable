import { Component, Input } from '@angular/core';
import { ConfigTextDirective } from '../../../directives/config-text/config-text.directive';

@Component({
  selector: 'app-heading-large',
  standalone: false,
  
  templateUrl: './heading-large.component.html',
  styleUrl: './heading-large.component.scss'
})
export class HeadingLargeComponent extends ConfigTextDirective {
}
