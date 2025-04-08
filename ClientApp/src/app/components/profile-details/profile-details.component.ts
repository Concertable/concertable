import { Component, Input, Output } from '@angular/core';
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
  updateLatLong(latLong: google.maps.LatLngLiteral | undefined) {
    if(this.user)
      if (latLong) {
        const { lat, lng } = latLong;
        this.user.latitude = lat;
        this.user.longitude = lng;
    }
    this.onChangeDetected();
  }

  @Output()
  get userChange() {
    return this.itemChange;
  }


  get user(): User | undefined {
    return this.item;
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }
  
  @Input()
  set user(user: User | undefined) {
    this.item = user;
  }

  setDetails(data: any): void {
    this.user = data['user']; 
  }

  onChangePassword() {
    if(this.user)
    this.authService.forgotPassword(this.user.email).subscribe();
  }
  
}
