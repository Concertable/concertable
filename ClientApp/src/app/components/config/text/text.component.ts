import { Component, Input, ElementRef, AfterViewChecked, ViewChild, AfterViewInit, AfterContentInit, OnChanges, SimpleChanges, Output, EventEmitter, OnDestroy, OnInit } from '@angular/core';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { ConfigTextDirective } from '../../../directives/config-text/config-text.directive';

@Component({
  selector: 'app-text',
  standalone: false,
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss'],
})
export class TextComponent extends ConfigTextDirective {
}
