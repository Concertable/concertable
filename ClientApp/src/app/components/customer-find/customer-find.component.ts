import { Component } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { SearchParams } from '../../models/search-params';

@Component({
  selector: 'app-customer-find',
  standalone: false,
  
  templateUrl: './customer-find.component.html',
  styleUrl: './customer-find.component.scss'
})
export class CustomerFindComponent {
  headerType: HeaderType = 'event';
}
