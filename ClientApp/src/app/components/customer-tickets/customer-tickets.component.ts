import { Component, Input, OnInit } from '@angular/core';
import { Ticket } from '../../models/ticket';
import { TicketService } from '../../services/ticket/ticket.service';
import { TicketViewType } from '../../models/ticket-view-type';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-customer-tickets',
  standalone: false,
  templateUrl: './customer-tickets.component.html',
  styleUrl: './customer-tickets.component.scss'
})
export class CustomerTicketsComponent implements OnInit {
  tickets: Ticket[] = [];

  constructor(
    private ticketService: TicketService, 
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const viewType = this.route.snapshot.data['viewType'];
    if (viewType === TicketViewType.Upcoming) 
      this.ticketService.getUserUpcoming().subscribe(tickets => {
        this.tickets = tickets;
      });
    if (viewType === TicketViewType.History) 
      this.ticketService.getUserHistory().subscribe(tickets => {
        this.tickets = tickets;
      });
    }

  onViewEvent(ticket: Ticket) {
    this.router.navigate([`/find/event/${ticket.event.id}`]);
  }

  onShowQr(ticket: Ticket) {
    
  }
}
