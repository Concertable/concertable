import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-number',
  standalone: false,
  templateUrl: './number.component.html',
  styleUrl: './number.component.scss'
})
export class NumberComponent {
  @Input() value?: number;
  @Input() editMode?: boolean = false;

  @Output() valueChange = new EventEmitter<number>();

  onInputChange(val: number | undefined) {
    if(val)
      this.valueChange.emit(val);
  }
}
