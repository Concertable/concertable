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

@Component({
  selector: 'app-mailbox',
  standalone: false,
  
  templateUrl: './mailbox.component.html',
  styleUrl: './mailbox.component.scss'
})
export class MailboxComponent implements OnInit, OnDestroy {
  messagesPage?: Pagination<Message>;
  pageParams: PaginationParams = {
    pageNumber: 1,
    pageSize: 5
  };
  unreadCount: number = 0;
  dropdownOpen = false;
  private subscriptions: Subscription[] = [];


  constructor(protected authService: AuthService, private messageService: MessageService, private router: Router, private route: ActivatedRoute) { }

  ngOnInit(): void {
    if (this.authService.isAuthenticated() && this.authService.isNotRole("Customer")) {
      this.subscriptions.push(this.messageService.getUnreadCountForUser().subscribe(count => {
        this.unreadCount = count;
      }));      
      this.loadMessages();
    }
  }
  
  loadMessages(): void {
    this.subscriptions.push(
      this.messageService.getForUser(this.pageParams).subscribe(response => {
        this.messagesPage = response;
      })
    );
  }
  
  onPageChange(event: PageEvent): void {
    this.pageParams.pageNumber = event.pageIndex + 1;
    this.pageParams.pageSize = event.pageSize;
    this.loadMessages();
  }
  

  onMailClick() {
    this.dropdownOpen = !this.dropdownOpen;
  }

  onShowMore() {

  }

  handleAction(action: Action) {
    if(action.name === "application") 
      this.router.navigateByUrl(`venue/my/applications/${action.id}`);
    if(action.name === "event")
      this.router.navigateByUrl(`artist/my/events/event/${action.id}`);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
