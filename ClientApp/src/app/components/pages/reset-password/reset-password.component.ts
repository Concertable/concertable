import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { ToastService } from '../../../services/toast/toast.service';
import { ResetPasswordRequest } from '../../../models/reset-password-request';

@Component({
  selector: 'app-reset-password',
  standalone: false,
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  request: ResetPasswordRequest = {
    userId: '',
    token: '',
    newPassword: ''
  };

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!userId || !token) {
      this.toastService.showError('Invalid reset link.');
      this.router.navigate(['/login']);
      return;
    }

    this.request.userId = userId;
    this.request.token = token;
  }

  resetPassword(): void {
    if (!this.request.newPassword) 
    {
      this.toastService.showError("Please type in a password");
      return;
    }

    this.authService.resetPassword(this.request).subscribe({
      next: () => {
        this.toastService.showSuccess('Password reset successful');
        this.router.navigate(['/login']);
      },
      error: () => {
        this.toastService.showError('Password reset failed');
      }
    });
  }
}

