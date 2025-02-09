import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Message, MessageAction } from '../../models/message';
import { MessageService } from '../../services/message/message.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-mailbox',
  standalone: false,
  
  templateUrl: './mailbox.component.html',
  styleUrl: './mailbox.component.scss'
})
export class MailboxComponent implements OnInit, OnDestroy {
  unreadCount: number = 0;
  messages: Message[] = [];
  dropdownOpen = false;
  private subscriptions: Subscription[] = [];


  constructor(protected authService: AuthService, private messageService: MessageService) { }

  ngOnInit(): void {
    this.subscriptions.push(this.authService.currentUser$.subscribe(user => {
      if(this.authService.isRole("VenueManager")){
        console.log("summary");
        this.subscriptions.push(this.messageService.getSummaryForUser().subscribe(messageSummary => {
          this.messages.push(...messageSummary.messages.data);
          this.unreadCount = messageSummary.unreadCount;
        }));
      }
    }));
  }

  onMailClick() {
    this.dropdownOpen = !this.dropdownOpen;
  }

  onShowMore() {

  }

  handleAction(action: MessageAction) {
    
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
