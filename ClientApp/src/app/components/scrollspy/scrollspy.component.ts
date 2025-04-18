import { Component, ElementRef, Input, OnInit, ViewChild, OnDestroy, AfterViewChecked, AfterViewInit } from '@angular/core';
import { NavItem } from '../../models/nav-item';
import { ActivatedRoute, Router } from '@angular/router';
import { fromEvent, Subscription } from 'rxjs';
import { throttleTime } from 'rxjs/operators';
import { StickyService } from '../../services/sticky/sticky.service';
import { ConfigNavbarService } from '../../services/config-navbar/config-navbar.service';

@Component({
  selector: 'app-scrollspy',
  standalone: false,
  templateUrl: './scrollspy.component.html',
  styleUrl: './scrollspy.component.scss'
})
export class ScrollspyComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() navItems: NavItem[] = [];
  isScrolled = false;
  activeFragment?: string;
  private sections!: HTMLElement[];
  private subscriptions: Subscription[] = [];
  protected configNavbarHeight?: number

  @ViewChild('nav', { static: true, read: ElementRef }) nav!: ElementRef;

  constructor(
    private route: ActivatedRoute,
    private configNavbarService: ConfigNavbarService,
    private stickyService: StickyService
  ) {}

  ngAfterViewInit(): void {
    this.sections = this.navItems
      .map(item => document.getElementById(item.fragment))
      .filter(section => section !== null) as HTMLElement[];



    this.subscriptions.push(this.route.fragment.subscribe(fragment => {
      if (fragment) {
        const element = document.getElementById(fragment);
        if (element) {
          window.scrollTo({
            top: element.offsetTop,
            behavior: 'smooth'
          });
        }
      }
    }));
  }

  ngOnInit(): void {
    this.configNavbarService.height$.subscribe(height => {
      this.configNavbarHeight = height;
      console.log("received height:", height);
    });

    this.subscriptions.push(
      this.stickyService.scrollY$.subscribe(scrollY => {
        this.updateActiveFragment(scrollY);
      })
    );
  }

  private updateActiveFragment(scrollY: number) {
    const scrollPosition = scrollY + (this.configNavbarHeight ?? 0) + 10;

  let currentSection = this.navItems[0]?.fragment;

  for (const section of this.sections) {
    if (section.offsetTop <= scrollPosition) {
      currentSection = section.id;
    }
  }

  if (this.activeFragment !== currentSection) {
    this.activeFragment = currentSection;
  }
  }

  get queryParams() {
    return this.route.snapshot.queryParams;
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe()); 
  }
}
