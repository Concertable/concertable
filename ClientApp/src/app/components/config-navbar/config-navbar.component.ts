import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-config-navbar',
  standalone: false,
  
  templateUrl: './config-navbar.component.html',
  styleUrl: './config-navbar.component.scss'
})
export class ConfigNavbarComponent {
  protected editMode: boolean = false;
  @Output() editModeChange = new EventEmitter<boolean>();

  onEditModeChange() {
    this.editMode = !this.editMode;
    this.editModeChange.emit(this.editMode);
  }
}
