import { Directive, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Data } from '@angular/router';
import { EventViewType } from '../../models/event-view-type';

@Directive({
  selector: '[appDashboard]',
  standalone: false
})
export abstract class DashboardDirective<T> implements OnInit {
  @Input() item?: T;

  EventViewType = EventViewType;

  constructor(protected route: ActivatedRoute){

  }
  
  ngOnInit(): void {
      this.route.data.subscribe(data => {
        this.setDetails(data);
      })
    }
  
  abstract setDetails(data: any): void;
}
