import { Component } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';
import { FindDirective } from '../../directives/find.directive';

@Component({
  selector: 'app-customer-dashboard',
  standalone: false,
  templateUrl: './customer-dashboard.component.html',
  styleUrl: './customer-dashboard.component.scss'
})
export class CustomerDashboardComponent extends FindDirective {
  handleSearch() {
    const params = this.searchParamsSerializerService.serialize(this.searchParams);
    this.router.navigate(['/find'],
      { queryParams: params }
    )
  }
}

