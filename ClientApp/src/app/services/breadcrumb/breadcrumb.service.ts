import { Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { Breadcrumb } from '../../models/breadcrumb';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
}
