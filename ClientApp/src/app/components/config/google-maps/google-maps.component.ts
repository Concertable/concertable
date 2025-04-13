import { AfterViewInit, Component, ElementRef, EventEmitter, HostListener, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { Coordinates } from '../../../models/coordinates';
import { GoogleMap } from '@angular/google-maps';

@Component({
  selector: 'app-google-maps',
  standalone: false,
  templateUrl: './google-maps.component.html',
  styleUrl: './google-maps.component.scss'
})
export class GoogleMapsComponent implements OnInit, OnChanges, AfterViewInit {
  mapWidth: string = '100%';
  mapHeight: string = '400px'; // fixed or you can compute based on width
  @Input() editMode?: boolean = false;
  @Input() lat?: number;
  @Input() lng?: number;
  @Output() latLongChange = new EventEmitter<google.maps.LatLngLiteral | undefined>();
  @Output() locationChange = new EventEmitter<{ county: string, town: string} | undefined>

  @ViewChild(GoogleMap) map?: GoogleMap;
  @ViewChild('container', { static: true }) container!: ElementRef;

  private resizeObserver?: ResizeObserver;

  options: google.maps.MapOptions = {
    center: { lat: 51, lng: 0.1 }
  }

  ngOnInit() {
    console.log(this.lat);
    this.onResize();
  }

  ngAfterViewInit() {
    // Create a ResizeObserver to monitor container's changes
    this.resizeObserver = new ResizeObserver(entries => {
      const entry = entries[0];
      // Get the container's current width
      const containerWidth = entry.contentRect.width;
      // Use the container width capped at 800px
      const newWidth = Math.min(containerWidth, 800);
      this.mapWidth = `${newWidth}px`;
    });
    // Start observing the container
    if (this.container && this.container.nativeElement) {
      this.resizeObserver.observe(this.container.nativeElement);
    }
  }
  

  @HostListener('window:resize', ['$event'])
    onResize(event?: Event) {
      // Calculate available width. This example uses document.body width.
      // Adjust this to target the specific container if needed.
      const currentWidth = window.innerWidth;
      
      // For example: use full width on mobile and restrict on larger screens
      if (currentWidth < 900) {
        this.mapWidth = '100%';
      } else {
        // Optionally constrain the width
        this.mapWidth = '800px'; 
      }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lat'] || changes['lng']) {
      if (this.lat !== undefined && this.lng !== undefined) {
        this.options.center = { lat: this.lat, lng: this.lng };
        if (this.map) {
          this.map.panTo(this.options.center);
        }
      }
    }
  }

  onLatLongChange(latLong: google.maps.LatLngLiteral | undefined) {
    if (latLong) {
      const { lat, lng } = latLong;
  
      this.lat = lat;
      this.lng = lng;
  
      this.latLongChange.emit({ lat, lng });
    } else {
      this.latLongChange.emit(undefined);
    }
  }
  

  onLocationChange(location?: { county: string, town: string }) {
    this.locationChange.emit(location);
  }
  
}
