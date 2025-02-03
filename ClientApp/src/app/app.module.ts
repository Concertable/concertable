import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';

import { LoginComponent } from './pages/login/login.component';
import { HomeComponent } from './pages/home/home.component';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { AuthLayoutComponent } from './components/auth-layout/auth-layout.component';
import { VenueDashboardComponent } from './pages/venue-dashboard/venue-dashboard.component';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';
import { CustomerHeaderComponent } from "./components/customer-header/customer-header.component";
import { VenueHeaderComponent } from './components/venue-header/venue-header.component';
import { YourVenueComponent } from './pages/your-venue/your-venue.component';
import { HeaderComponent } from './components/header/header.component';
import { ProfileComponent } from './components/profile/profile.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { GoogleMapsModule } from '@angular/google-maps';
import { GoogleMapsComponent } from './components/google-maps/google-maps.component';
import { VenueDetailsComponent } from './components/venue-details/venue-details.component';
import { ConfigHeaderComponent } from './components/config-header/config-header.component';
import { ConfigButtonComponent } from './components/config-button/config-button.component';
import { ListingComponent } from './components/listing/listing.component';
import { ListingsComponent } from './components/listings/listings.component';
import { EditableVenueDetailsComponent } from './components/editable-venue-details/editable-venue-details.component';
import { DetailsComponent } from './components/details/details.component';
import { ScrollspyComponent } from './components/scrollspy/scrollspy.component';
import { VenueEventsComponent } from './components/venue-events/venue-events.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HomeComponent,
    AuthLayoutComponent,
    VenueDashboardComponent,
    CustomerHeaderComponent,
    YourVenueComponent,
    VenueHeaderComponent,
    YourVenueComponent,
    HeaderComponent,
    ProfileComponent,
    GoogleMapsComponent,
    VenueDetailsComponent,
    ConfigHeaderComponent,
    ConfigButtonComponent,
    ListingsComponent,
    ListingComponent,
    EditableVenueDetailsComponent,
    DetailsComponent,
    ScrollspyComponent,
    VenueEventsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    MatIconModule,
    MatMenuModule,
    GoogleMapsModule
],
  providers: [
    provideHttpClient(
      withInterceptorsFromDi()
    ),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    provideAnimationsAsync(),
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
