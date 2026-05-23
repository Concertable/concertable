import { Directive, EventEmitter, Input, Output } from '@angular/core';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Directive({
  selector: '[appConfigText]',
  standalone: false
})
export class ConfigTextDirective {
  @Input() editMode? = false;
  @Input() content?: string;
  @Input() label?: string;
  @Input() whiteOutline?: boolean;
  @Output() contentChange = new EventEmitter<string>();

  onContentChange(content: string) {
    this.contentChange.emit(content);
  }
}
