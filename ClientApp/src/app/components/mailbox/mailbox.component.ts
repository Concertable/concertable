import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Message, MessageAction } from '../../models/message';
import { MessageService } from '../../services/message/message.service';
import { Subscription } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { Pagination } from '../../models/pagination';
import { PaginationParams } from '../../models/pagination-params';
import { PageEvent } from '@angular/material/paginator';
import { Action } from '../../models/action';
import { PaginationHandler } from '../../shared/handler/pagination-handler';

@Component({
  selector: 'app-mailbox',
  standalone: false,
  
  templateUrl: './mailbox.component.html',
  styleUrl: './mailbox.component.scss'
})
export class MailboxComponent extends PaginationHandler<Message> implements OnInit {
  unreadCount: number = 0;
  dropdownOpen = false;

  constructor(
    protected authService: AuthService,
    private messageService: MessageService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    super(); 
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated() && this.authService.isNotRole("Customer")) {
      this.subscriptions.push(this.messageService.getUnreadCountForUser().subscribe(count => {
        this.unreadCount = count;
      }));      
      this.loadPage();
    }
  }

  override loadPage(): void {
    this.subscriptions.push(
      this.messageService.getForUser(this.pageParams).subscribe(response => {
        this.paginatedData = response;
      })
    );
  }

  onMailClick() {
    this.dropdownOpen = !this.dropdownOpen;
    this.markReadMessages();
  }

  handleAction(action: Action) {
    if(action.name === "application") 
      this.router.navigateByUrl(`venue/my/applications/${action.id}`);
    if(action.name === "event")
      this.router.navigateByUrl(`artist/my/events/event/${action.id}`);
  }

  markReadMessages() {
    console.log("??")
    const unreadIds = this.paginatedData?.data
    ?.filter(m => !m.read)
    .map(m => m.id) ?? [];

    if (unreadIds.length === 0) return;

    this.messageService.markAsRead(unreadIds).subscribe(uc => this.unreadCount = uc);
  }

  override onPageChange(params: PaginationParams): void {
      super.onPageChange(params);
      this.markReadMessages();
  }

  onDelete(message: Message) {

  }
}
