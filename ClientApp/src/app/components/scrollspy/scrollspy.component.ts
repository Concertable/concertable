import { Component, ElementRef, Input, OnInit, ViewChild, OnDestroy, AfterViewChecked, AfterViewInit } from '@angular/core';
import { NavItem } from '../../models/nav-item';
import { ActivatedRoute, Router } from '@angular/router';
import { fromEvent, Subscription } from 'rxjs';
import { throttleTime } from 'rxjs/operators';

@Component({
  selector: 'app-scrollspy',
  standalone: false,
  templateUrl: './scrollspy.component.html',
  styleUrl: './scrollspy.component.scss'
})
export class ScrollspyComponent implements AfterViewInit, OnDestroy {
  @Input() navItems: NavItem[] = [];
  isScrolled = false;
  activeFragment?: string;
  navOffsetTop!: number;
  private sections!: HTMLElement[];
  private subscriptions: Subscription[] = [];

  @ViewChild('nav', { static: true, read: ElementRef }) nav!: ElementRef;

  constructor(private router: Router, private route: ActivatedRoute) {}

  ngAfterViewInit(): void {
    this.sections = this.navItems
      .map(item => document.getElementById(item.fragment))
      .filter(section => section !== null)

    this.navOffsetTop = this.nav.nativeElement.offsetTop;

    this.subscriptions.push(this.route.fragment.subscribe(fragment => {
      if (fragment) {
        const element = document.getElementById(fragment);
        if (element) {
          const navbarOffset = 80; 
          window.scrollTo({
            top: element.offsetTop - navbarOffset,
            behavior: 'smooth'
          });
        }
      }
    }));

    this.subscriptions.push(fromEvent(window, 'scroll')
      .pipe(throttleTime(100))
      .subscribe(() => this.handleScroll()));
  }

  get queryParams() {
    return this.route.snapshot.queryParams;
  }

  private handleScroll() {
    this.isScrolled = window.scrollY > this.navOffsetTop;

    let currentSection = this.navItems[0]?.fragment; 
    const scrollPosition = window.scrollY; 

    for (let section of this.sections) {
      console.log(section.offsetTop);
      if (section.offsetTop <= scrollPosition) {
        currentSection = section.id;
      }
    }

    if (this.activeFragment !== currentSection) {
      this.activeFragment = currentSection;
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe()); 
  }
}
