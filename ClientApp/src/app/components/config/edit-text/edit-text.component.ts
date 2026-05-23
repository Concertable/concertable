import { Component, EventEmitter, Input, Output } from '@angular/core';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'app-edit-text',
  standalone: false,
  templateUrl: './edit-text.component.html',
  styleUrl: './edit-text.component.scss'
})
export class EditTextComponent {
  @Input() editMode? = false;
  @Input() content?: string;
  @Input() label?: string;
  @Output() contentChange = new EventEmitter<string>();

  private contentSubject = new Subject<string>();

  ngOnInit(): void {
    this.contentSubject
      .pipe(
        debounceTime(300), 
        distinctUntilChanged() 
      )
      .subscribe((updatedContent) => {
        this.contentChange.emit(updatedContent);
      });
  }
  
  onContentChange(content: string) {
    this.contentSubject.next(content);
  }

  ngOnDestroy(): void {
    this.contentSubject.complete()
  }
}
