import { Directive, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';
import { ActivatedRoute, Router } from '@angular/router';

@Directive({
  selector: '[appCreateItem]',
  standalone: false
})
export abstract class CreateItemDirective<T extends Venue | Artist> implements OnInit {
  item?: T;

  constructor(private router: Router, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.createDefaultItem();
  }

  abstract createDefaultItem(): void;

  abstract create(item: T): Observable<T>;

  abstract showCreated(name: string): void;

  navigateToItem() {
    this.router.navigate(['../my'], { relativeTo: this.route });
  }

  createChanges() {
    if(this.item)
      this.create(this.item).subscribe(v => {
    this.showCreated(v.name);
    this.navigateToItem();
  })
  }
}
