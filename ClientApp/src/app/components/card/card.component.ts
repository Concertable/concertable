import {
  Component,
  ContentChild,
  AfterContentInit,
  ElementRef,
  ViewChild,
  Input
} from '@angular/core';

@Component({
  selector: 'app-card',
  templateUrl: './card.component.html',
  standalone: false,
  styleUrls: ['./card.component.scss'],
})
export class CardComponent {
  @Input() isEmpty = false;
}
