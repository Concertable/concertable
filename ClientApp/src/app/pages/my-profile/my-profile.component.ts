import { Component } from '@angular/core';
import { ConfigDirective } from '../../directives/config/config.directive';
import { User } from '../../models/user';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-my-profile',
  standalone: false,
  templateUrl: './my-profile.component.html',
  styleUrl: './my-profile.component.scss'
})
export class MyProfileComponent extends ConfigDirective<User> {
  constructor(
    route: ActivatedRoute, 
  ) {
    super(route);
   }

  get user(): User | undefined {
      return this.item;
    }
  
  set user(value: User | undefined) {
    this.item = value;
  }

  setDetails(data: any): void {
    console.log(data);
    this.user = data['user'];
  }

  update(item: User): Observable<User> {
    //return this.artistService.update(artist);
    throw new Error('Method not implemented.');
  }

  showUpdated(item: User): void {
    throw new Error('Method not implemented.');
  }

}
