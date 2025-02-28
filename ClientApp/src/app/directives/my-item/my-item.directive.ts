import { Directive, EventEmitter, OnInit, Output } from '@angular/core';
import { clone, cloneDeep } from 'lodash';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';
import { Event } from '../../models/event';
import { ActivatedRoute } from '@angular/router';
import { User } from '../../models/user';

@Directive({
  selector: '[appMyItem]',
  standalone: false
})
export abstract class MyItemDirective<T extends Venue | Artist | Event | User> implements OnInit {
  protected item?: T;
  protected originalItem?: T;
  protected editMode: boolean = false;
  protected saveable: boolean = false;

  constructor(private route: ActivatedRoute) { }

  // Abstract method for getting details, to be implemented in child classes
  abstract setDetails(data: any): void;

  abstract update(item: T): Observable<T>;

  abstract showUpdated(item: T): void

  onEditModeChange(newValue: boolean) {
    this.editMode = newValue;
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

  onEntityChange(entity: T) {
    this.saveable = true;
    this.item = entity;
  }
}
