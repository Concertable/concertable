import { Component, OnInit } from '@angular/core';
import { PaginationHandler } from '../../shared/handler/pagination-handler';
import { Transaction } from '../../models/transaction';
import { PaginationParams } from '../../models/pagination-params';
import { TransactionService } from '../../services/transaction/transaction.service';

@Component({
  selector: 'app-transactions',
  standalone: false,
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss'
})
export class TransactionsComponent extends PaginationHandler<Transaction> implements OnInit {

  constructor(private transactionService: TransactionService) {
    super();
    
  }

  ngOnInit(): void {
      this.loadPage();
  }

  loadPage(): void {
    this.subscriptions.push(
      this.transactionService.getTransactions(this.pageParams).subscribe(response => {
        this.paginatedData = response;
      })
    );
  }
}
