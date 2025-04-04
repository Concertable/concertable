import { Component, Input } from '@angular/core';
import { DetailsDirective } from '../../directives/details/details.directive';
import { User } from '../../models/user';
import { Coordinates } from '../../models/coordinates';

@Component({
  selector: 'app-profile-details',
  standalone: false,
  templateUrl: './profile-details.component.html',
  styleUrl: './profile-details.component.scss'
})
export class ProfileDetailsComponent extends DetailsDirective<User> {
updateLatLong($event: Coordinates|undefined) {
throw new Error('Method not implemented.');
}
  onLocationChange($event: google.maps.LatLngLiteral|undefined) {
  throw new Error('Method not implemented.');
  }
  onLocationValueChange($event: { county: string; town: string; }|undefined) {
  throw new Error('Method not implemented.');
  }

  get user(): User | undefined {
    return this.entity;
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }
  
  @Input()
  set user(user: User | undefined) {
    this.entity = user;
  }

  setDetails(data: any): void {
    this.user = data['user']; 
  }
}
