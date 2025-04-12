import { Component, Input, OnInit } from '@angular/core';
import { Ticket } from '../../models/ticket';
import { TicketService } from '../../services/ticket/ticket.service';
import { TicketViewType } from '../../models/ticket-view-type';
import { ActivatedRoute, Router } from '@angular/router';
import { QrCodeDialogComponent } from '../../qr-code-dialog/qr-code-dialog.component';
import { MatDialog } from '@angular/material/dialog';

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
    private dialog: MatDialog
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

  onShowQr(ticket: Ticket): void {
    const dialogRef = this.dialog.open(QrCodeDialogComponent, {
      width: '300px',
      maxWidth: '90vw',
    });
    dialogRef.afterOpened().subscribe(() => {
      dialogRef.componentInstance.qrCode = ticket.qrCode;
    });
  }
  
}
