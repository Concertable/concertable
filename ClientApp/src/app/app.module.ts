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
import { MAT_DATE_LOCALE, MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card'
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider'; 
import { MatPaginatorModule } from '@angular/material/paginator';


import { GoogleMapsModule } from '@angular/google-maps';
import { GoogleMapsComponent } from './components/config/google-maps/google-maps.component';

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
import { EventDetailsComponent } from './pages/event-details/event-details.component';
import { ManagerItemNotFoundComponent } from './components/manager-item-not-found/manager-item-not-found.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations'; 
import { ToastrModule } from 'ngx-toastr';
import { ConfigTextDirective } from './directives/config-text/config-text.directive';
import { EditTextComponent } from './components/config/edit-text/edit-text.component';
import { ListingApplicationsComponent } from './pages/listing-applications/listing-applications.component';
import { ArtistSummaryComponent } from './components/artist-summary/artist-summary.component';
import { MyEventsComponent } from './pages/my-events/my-events.component';
import { MyEventComponent } from './pages/my-event/my-event.component';
import { ItemEventsComponent } from './components/item-events/item-events.component';
import { ArtistEventsComponent } from './components/artist-events/artist-events.component';
import { EventCheckoutComponent } from './pages/event-checkout/event-checkout.component';
import { ReviewSummaryComponent } from './components/review-summary/review-summary.component';
import { CardDetailsComponent } from './components/card-details/card-details.component';
import { ProfileDetailsComponent } from './pages/profile-details/profile-details.component';
import { MyTicketsComponent } from './pages/my-tickets/my-tickets.component';
import { PaymentDetailsComponent } from './components/payment-details/payment-details.component';
import { ListingApplicationCheckoutComponent } from './pages/listing-application-checkout/listing-application-checkout.component';

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
    EventDetailsComponent,
    ManagerItemNotFoundComponent,
    ConfigTextDirective,
    EditTextComponent,
    ListingApplicationsComponent,
    ArtistSummaryComponent,
    MyEventsComponent,
    MyEventComponent,
    ItemEventsComponent,
    ArtistEventsComponent,
    EventCheckoutComponent,
    ReviewSummaryComponent,
    CardDetailsComponent,
    ProfileDetailsComponent,
    MyTicketsComponent,
    PaymentDetailsComponent,
    ListingApplicationCheckoutComponent
  ],
  imports: [
    BrowserAnimationsModule, // Required for animations
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right', // Position of toast (top-right, bottom-right, etc.)
      timeOut: 5000, // Auto-dismiss after 3 seconds
      closeButton: true,
      progressBar: true
    }),
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
    MatCardModule,
    MatButtonModule,
    MatSliderModule,
    MatPaginatorModule
],
  providers: [
    provideHttpClient(
      withInterceptorsFromDi()
    ),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    provideAnimationsAsync(),
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
