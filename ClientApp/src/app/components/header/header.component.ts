import { Component, Input } from '@angular/core';
import { Header } from '../../models/header';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderType } from '../../models/header-type';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';

@Component({
  selector: 'app-header',
  standalone: false,
  
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent<T extends Header> {
  @Input() header?: T;
  @Input() headerType?: HeaderType;

  constructor(
    protected blobStorageService: BlobStorageService,
    private router: Router, 
    private route: ActivatedRoute) { }

  onClick(): void {
    console.log("heee")
    if(this.header)
      this.router.navigate([this.headerType], { 
        relativeTo: this.route,
        queryParams: { id: this.header.id }  
    });
  }
}
