import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-input',
  standalone: false,
  templateUrl: './input.component.html',
  styleUrl: './input.component.scss'
})
export class InputComponent {
  @Input() label: string = '';
  @Input() content?: string;
  @Output() contentChange = new EventEmitter<string>();

  onContentChange(value: string) {
    this.contentChange.emit(value);
  }
}
