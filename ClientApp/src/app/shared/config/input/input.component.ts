import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ConfigTextDirective } from '../../../directives/config-text/config-text.directive';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { InputDirective } from '../../../directives/input.directive';

@Component({
  selector: 'app-input',
  standalone: false,
  templateUrl: './input.component.html',
  styleUrl: './input.component.scss'
})
export class InputComponent extends InputDirective {
}
