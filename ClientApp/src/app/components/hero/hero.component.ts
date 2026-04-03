import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-hero',
  standalone: false,
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.scss'
})
export class HeroComponent {
  @Input() title?: string;
  @Input() titleStyle: { [key: string]: string } = {};
}
