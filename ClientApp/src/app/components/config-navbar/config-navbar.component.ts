import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-config-navbar',
  standalone: false,
  
  templateUrl: './config-navbar.component.html',
  styleUrl: './config-navbar.component.scss'
})
export class ConfigNavbarComponent {
  @Input() editMode: boolean = false;
  @Output() editModeChange = new EventEmitter<boolean>();
  @Input() saveable: boolean = false;
  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onEditModeChange() {
    this.editMode = !this.editMode;
    this.editModeChange.emit(this.editMode);
  }

  onSave() {
    this.save.emit();
  }

  onCancel() {
    this.cancel.emit()
  }
}
