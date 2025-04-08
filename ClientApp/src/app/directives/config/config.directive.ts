import { Directive, EventEmitter, OnInit, Output } from '@angular/core';
import { clone, cloneDeep } from 'lodash';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';
import { Event } from '../../models/event';
import { ActivatedRoute } from '@angular/router';
import { User } from '../../models/user';
import { Preference } from '../../models/preference';
import { Coordinates } from '../../models/coordinates';
import { ToastService } from '../../services/toast/toast.service';

@Directive({
  selector: '[appConfig]',
  standalone: false
})
export abstract class ConfigDirective<T extends Venue | Artist | Event | User | Preference | Coordinates> implements OnInit {
  protected item?: T;
  protected originalItem?: T;
  protected editMode: boolean = false;
  protected saveable: boolean = false;

  constructor(protected route: ActivatedRoute) { }

  // Abstract method for getting details, to be implemented in child classes
  abstract setDetails(data: any): void;

  abstract update(item: T): Observable<T>;

  abstract showUpdated(item: T): void

  onEditModeChange(editMode: boolean) {
    this.editMode = editMode;
  }

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.setDetails(data);
      this.originalItem = cloneDeep(this.item);
    })
  }

  cancelChanges() {
    this.item = cloneDeep(this.originalItem);
    this.saveable = false;
    this.editMode = false;
  }

  saveChanges() {
    if(this.item)
      this.update(this.item).subscribe(v => {
        this.item = v;
        this.originalItem = cloneDeep(this.item);
        this.showUpdated(v)
    })
  }

  onItemChange() {
    this.saveable = true;
    console.log("Item", this.item);
  }
}
