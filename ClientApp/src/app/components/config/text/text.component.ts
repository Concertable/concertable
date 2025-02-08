import { Component, Input, ElementRef, AfterViewChecked, ViewChild, AfterViewInit, AfterContentInit, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-text',
  standalone: false,
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss'],
})
export class TextComponent {
  @Input() editMode? = false;
  @Input() content?: string;
  @Input() label?: string;
}
