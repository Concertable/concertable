import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-config-header',
  standalone: false,
  
  templateUrl: './config-header.component.html',
  styleUrl: './config-header.component.scss'
})
export class ConfigHeaderComponent {
  protected editMode: boolean = false;
  @Output() editModeChange = new EventEmitter<boolean>();

  onEditModeChange() {
    this.editMode = !this.editMode;
    this.editModeChange.emit(this.editMode);
  }
}
