import { Component } from '@angular/core';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-customer-find',
  standalone: false,
  
  templateUrl: './customer-find.component.html',
  styleUrl: './customer-find.component.scss'
})
export class CustomerFindComponent {
  searchType?: HeaderType;
}
