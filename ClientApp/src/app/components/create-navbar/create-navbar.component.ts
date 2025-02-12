import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-create-navbar',
  standalone: false,
  
  templateUrl: './create-navbar.component.html',
  styleUrl: './create-navbar.component.scss'
})
export class CreateNavbarComponent {
  @Output() create = new EventEmitter<void>();

  onCreate() {
    this.create.emit();
  }
}
