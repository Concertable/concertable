import { Directive, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';
import { ActivatedRoute, Router } from '@angular/router';
import { Preference } from '../../models/preference';

@Directive({
  selector: '[appCreateItem]',
  standalone: false
})
export abstract class CreateItemDirective<T extends Venue | Artist | Preference> implements OnInit {
  item?: T;
  image?: File

  constructor(protected router: Router, protected route: ActivatedRoute) { }

  ngOnInit(): void {
    this.createDefaultItem();
  }

  abstract createDefaultItem(): void;

  abstract create(item: T): Observable<T>;

  abstract showCreated(item: T): void;

  navigateToItem() {
    this.router.navigate(['../my'], { relativeTo: this.route });
  }

  createChanges() {
    if(this.item)
      this.create(this.item).subscribe(i => {
      this.showCreated(i)
      this.navigateToItem();
    })
  }

  updateImage(image: File) {
    this.image = image;
  }
}
