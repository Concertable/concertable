import { Component, Input } from '@angular/core';
import { Header } from '../../models/header';

@Component({
  selector: 'app-headers',
  standalone: false,
  
  templateUrl: './headers.component.html',
  styleUrl: './headers.component.scss'
})
export class HeadersComponent<T extends Header> {
  @Input() headers: T[] = [];
}
