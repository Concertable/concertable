import { Component, Input, OnInit } from '@angular/core';
import { NavItem } from '../../models/nav-item';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-scrollspy',
  standalone: false,
  
  templateUrl: './scrollspy.component.html',
  styleUrl: './scrollspy.component.scss'
})
export class ScrollspyComponent implements OnInit{
  @Input() navItems: NavItem[] = [];

  constructor(private router: Router, private route: ActivatedRoute) { }

  /*
  By default, angular doesnt handle fragments if the url 
  doesnt change, so we have to do it manually
  */
  ngOnInit(): void {
    this.route.fragment.subscribe(fragment => {
      const element = document.getElementById(fragment ?? '')
      if(element)
        element.scrollIntoView();
    })
  }
}
