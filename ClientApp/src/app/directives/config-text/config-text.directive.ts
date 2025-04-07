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
