import { Component } from '@angular/core';
import { ConfigDirective } from '../../directives/config/config.directive';
import { User } from '../../models/user';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { ToastService } from '../../services/toast/toast.service';
import { UserService } from '../../services/user.service';
import { UserToastService } from '../../services/toast/user-toast.service';

@Component({
  selector: 'app-my-profile',
  standalone: false,
  templateUrl: './my-profile.component.html',
  styleUrl: './my-profile.component.scss'
})
export class MyProfileComponent extends ConfigDirective<User> {
  prevEmail?: string;
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private userToastService: UserToastService,
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

  override ngOnInit(): void {
      super.ngOnInit();
      this.route.queryParams.subscribe(params => {
        this.editMode = params['editMode'] === 'true';
      });
  }

  setDetails(data: any): void {
    this.user = data['user'];
    this.prevEmail = this.user?.email;
  }

  update(user: User): Observable<User> {
    return this.userService.updateLocation(user.id, user.latitude, user.longitude)
  }

  showUpdated(item: User): void {
    this.userToastService.showUpdated();
  }

  override saveChanges() {
    if (!this.user || !this.originalItem) return;
  
    if (this.user?.email !== this.prevEmail) {
      // Send email confirmation instead of directly saving
      this.authService.requestEmailChange(this.user?.email).subscribe();
    }
    
    // For all other cases, run base save logic
    super.saveChanges();
  }

}
