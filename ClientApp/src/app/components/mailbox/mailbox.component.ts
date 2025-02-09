import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Message, MessageAction } from '../../models/message';
import { MessageService } from '../../services/message/message.service';

@Component({
  selector: 'app-mailbox',
  standalone: false,
  
  templateUrl: './mailbox.component.html',
  styleUrl: './mailbox.component.scss'
})
export class MailboxComponent implements OnInit {
  unreadCount: number = 0;
  messages: Message[] = [];
  dropdownOpen = false;

  constructor(protected authService: AuthService, private messageService: MessageService) { }

  ngOnInit(): void {
    console.log("pretest");
    console.log("f",this.authService.user())
    if(this.authService.user()){
      console.log("summary");
      this.messageService.getSummaryForUser().subscribe(messageSummary => {
        this.messages.push(...messageSummary.messages.data);
        this.unreadCount = messageSummary.unreadCount;
      });
    }
  }

  onMailClick() {
    this.dropdownOpen = !this.dropdownOpen;
  }

  onShowMore() {

  }

  handleAction(action: MessageAction) {
    
  }
}
