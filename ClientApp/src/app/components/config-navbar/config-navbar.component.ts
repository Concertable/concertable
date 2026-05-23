import { AfterViewChecked, AfterViewInit, Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { ConfigNavbarService } from '../../services/config-navbar/config-navbar.service';

@Component({
  selector: 'app-config-navbar',
  standalone: false,
  
  templateUrl: './config-navbar.component.html',
  styleUrl: './config-navbar.component.scss'
})
export class ConfigNavbarComponent implements AfterViewInit, OnDestroy {
  @Input() editMode: boolean = false;
  @Output() editModeChange = new EventEmitter<boolean>();
  @Input() saveable: boolean = false;
  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  @ViewChild('nav', { static: true, read: ElementRef }) nav!: ElementRef;
  
  subscriptions: Subscription[] = [];

  constructor(private configNavbarService: ConfigNavbarService) { }

  ngAfterViewInit(): void {
    this.configNavbarService.setHeight(this.nav.nativeElement.offsetHeight);
  }

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

  ngOnDestroy(): void {
    this.configNavbarService.setHeight(undefined);
  }
}
