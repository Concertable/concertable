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
import { CustomerNavbarComponent } from "./components/customer-navbar/customer-navbar.component";
import { VenueNavbarComponent } from './components/venue-navbar/venue-navbar.component';
import { MyVenueComponent } from './pages/my-venue/my-venue.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { ProfileComponent } from './components/profile/profile.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card'

import { GoogleMapsModule } from '@angular/google-maps';
import { GoogleMapsComponent } from './components/google-maps/google-maps.component';

import { VenueDetailsComponent } from './pages/venue-details/venue-details.component';
import { ConfigNavbarComponent } from './components/config-navbar/config-navbar.component';
import { ConfigButtonComponent } from './components/config-button/config-button.component';
import { ListingComponent } from './components/listing/listing.component';
import { ListingsComponent } from './components/listings/listings.component';
import { EditableVenueDetailsComponent } from './components/editable-venue-details/editable-venue-details.component';
import { DetailsComponent } from './components/details/details.component';
import { ScrollspyComponent } from './components/scrollspy/scrollspy.component';
import { VenueEventsComponent } from './components/venue-events/venue-events.component';
import { ArtistDashboardComponent } from './pages/artist-dashboard/artist-dashboard.component';
import { ArtistNavbarComponent } from './components/artist-navbar/artist-navbar.component';
import { ArtistFindComponent } from './pages/venue-find/artist-find.component';
import { SearchComponent } from './components/search/search.component';
import { DatePickerComponent } from './components/date-picker/date-picker.component';
import { LocationSearchComponent } from './components/location-search/location-search.component';
import { FindComponent } from './pages/find/find.component';
import { SearchResultsComponent } from './components/search-results/search-results.component';
import { SearchResultHeaderComponent } from './components/search-result-header/search-result-header.component';
import { EventHeaderComponent } from './components/event-header/event-header.component';
import { HeaderComponent } from './components/header/header.component';
import { VenueHeaderComponent } from './components/venue-header/venue-header.component';
import { ArtistHeaderComponent } from './components/artist-header/artist-header.component';
import { ArtistHeadersComponent } from './components/artist-headers/artist-headers.component';
import { VenueHeadersComponent } from './components/venue-headers/venue-headers.component';
import { EventHeadersComponent } from './components/event-headers/event-headers.component';
import { HeadersComponent } from './components/headers/headers.component';
import { TextComponent } from './components/config/text/text.component';
import { TextareaComponent } from './components/config/textarea/textarea.component';
import { ImageComponent } from './components/config/image/image.component';
import { HeadingLargeComponent } from './components/config/heading-large/heading-large.component';
import { HeadingMediumComponent } from './components/config/heading-medium/heading-medium.component';
import { LocationComponent } from './components/config/location/location.component';
import { ExtraOptions } from '@angular/router';
import { VenueFindComponent } from './components/venue-find/venue-find.component';
import { ArtistDetailsComponent } from './pages/artist-details/artist-details.component';
import { CustomerFindComponent } from './components/customer-find/customer-find.component';
import { FilterComponent } from './components/filter/filter.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { RegisterComponent } from './pages/register/register.component';
import { MyArtistComponent } from './pages/my-artist/my-artist.component';
import { CreateArtistComponent } from './pages/create-artist/create-artist.component';
import { CreateVenueComponent } from './pages/create-venue/create-venue.component';
import { CreateNavbarComponent } from './components/create-navbar/create-navbar.component';
import { MailboxComponent } from './components/mailbox/mailbox.component';

const routerOptions: ExtraOptions = {
  anchorScrolling: 'enabled',
  scrollPositionRestoration: 'enabled' // Restores scroll position when navigating back
};

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HomeComponent,
    AuthLayoutComponent,
    VenueDashboardComponent,
    CustomerNavbarComponent,
    MyVenueComponent,
    VenueNavbarComponent,
    NavbarComponent,
    ProfileComponent,
    GoogleMapsComponent,
    VenueDetailsComponent,
    ConfigNavbarComponent,
    ConfigButtonComponent,
    ListingsComponent,
    ListingComponent,
    EditableVenueDetailsComponent,
    DetailsComponent,
    ScrollspyComponent,
    VenueEventsComponent,
    ArtistDashboardComponent,
    ArtistNavbarComponent,
    ArtistFindComponent,
    SearchComponent,
    DatePickerComponent,
    LocationSearchComponent,
    FindComponent,
    SearchResultsComponent,
    SearchResultHeaderComponent,
    ArtistHeadersComponent,
    VenueHeadersComponent,
    EventHeadersComponent,
    HeaderComponent,
    ArtistHeaderComponent,
    VenueHeaderComponent,
    EventHeaderComponent,
    HeadersComponent,
    TextComponent,
    TextareaComponent,
    ImageComponent,
    HeadingLargeComponent,
    HeadingMediumComponent,
    LocationComponent,
    VenueFindComponent,
    ArtistDetailsComponent,
    CustomerFindComponent,
    FilterComponent,
    SidebarComponent,
    RegisterComponent,
    MyArtistComponent,
    CreateArtistComponent,
    CreateVenueComponent,
    CreateNavbarComponent,
    MailboxComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    MatIconModule,
    MatMenuModule,
    GoogleMapsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatAutocompleteModule,
    MatSelectModule,
    MatSidenavModule,
    MatToolbarModule,
    MatBadgeModule,
    MatListModule,
    MatCardModule
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
