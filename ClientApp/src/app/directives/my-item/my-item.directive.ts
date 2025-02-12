import { Directive, OnInit } from '@angular/core';
import { cloneDeep } from 'lodash';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';

@Directive({
  selector: '[appMyItem]',
  standalone: false
})
export abstract class MyItemDirective<T extends Venue | Artist> implements OnInit {
  protected item?: T;
  private originalItem?: T;
  protected editMode: boolean = false;
  protected saveable: boolean = false;


  // Abstract method for getting details, to be implemented in child classes
  abstract getDetailsForCurrentUser(): Observable<T>;

  abstract update(item: T): Observable<T>;

  abstract showUpdated(name: string): void;

  onEditModeChange(newValue: boolean) {
    this.editMode = newValue;
  }

  ngOnInit(): void {
    this.getDetailsForCurrentUser().subscribe((item: T) => {
      this.item = item;
      this.originalItem = cloneDeep(this.item);
    });
  }

  cancelChanges() {
    this.item = cloneDeep(this.originalItem);
    this.saveable = false;
    this.editMode = false;
  }

  saveChanges() {
    console.log(this.item)
    if(this.item)
      this.update(this.item).subscribe(v => this.showUpdated(v.name))
  }

  onEntityChange(entity: T) {
    this.saveable = true;
    this.item = entity;
  }
}
