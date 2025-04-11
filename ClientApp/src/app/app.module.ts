import { NgModule, CUSTOM_ELEMENTS_SCHEMA, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';

import { TextFieldModule } from '@angular/cdk/text-field';

import { LoginComponent } from './pages/login/login.component';
import { HomeComponent } from './pages/home/home.component';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { AuthLayoutComponent } from './shared/layouts/auth-layout/auth-layout.component';
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
import { MatTimepickerModule } from '@angular/material/timepicker';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';


import { GoogleMapsModule } from '@angular/google-maps';
import { GoogleMapsComponent } from './components/config/google-maps/google-maps.component';

import { VenueDetailsComponent } from './pages/venue-details/venue-details.component';
import { ConfigNavbarComponent } from './components/config-navbar/config-navbar.component';
import { ConfigButtonComponent } from './components/config-button/config-button.component';
import { ListingComponent } from './components/listing/listing.component';
import { ListingsComponent } from './components/listings/listings.component';
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
import { LocationDisplayComponent } from './components/config/location-display/location-display.component';
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
import { ConfigItemNotFoundComponent } from './components/config-item-not-found/config-item-not-found.component';

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
import { CardDetailsComponent } from './components/card-details/card-details.component';
import { ProfileMenuComponent } from './pages/profile-menu/profile-menu.component';
import { MyTicketsComponent } from './pages/my-tickets/my-tickets.component';
import { PaymentDetailsComponent } from './components/payment-details/payment-details.component';
import { ListingApplicationCheckoutComponent } from './pages/listing-application-checkout/listing-application-checkout.component';
import { CustomerDashboardComponent } from './pages/customer-dashboard/customer-dashboard.component';
import { BreadcrumbComponent } from './components/breadcrumb/breadcrumb.component';
import { VenueHomeComponent } from './pages/venue-home/venue-home.component';
import { ArtistHomeComponent } from './pages/artist-home/artist-home.component';
import { TimePickerComponent } from './components/time-picker/time-picker.component';
import { DropDownComponent } from './components/drop-down/drop-down.component';
import { CardComponent } from './components/card/card.component';
import { VenueReviewsComponent } from './components/venue-reviews/venue-reviews.component';
import { ArtistReviewsComponent } from './components/artist-reviews/artist-reviews.component';
import { EventReviewsComponent } from './components/event-reviews/event-reviews.component';
import { FooterComponent } from './components/footer/footer.component';
import { ProfileDetailsComponent } from './components/profile-details/profile-details.component';
import { MyProfileComponent } from './pages/my-profile/my-profile.component';
import { DashboardLayoutComponent } from './shared/layouts/dashboard-layout/dashboard-layout.component';
import { MyPreferenceComponent } from './components/my-preference/my-preference.component';
import { SelectorComponent } from './components/selector/selector.component';
import { SliderComponent } from './components/slider/slider.component';
import { PreferenceDetailsComponent } from './components/preference-details/preference-details.component';
import { EventCarouselComponent } from './components/event-carousel/event-carousel.component';
import { BadgeComponent } from './components/badge/badge.component';
import { PriceComponent } from './components/config/price/price.component';
import { NumberComponent } from './components/config/number/number.component';
import { InitService } from './services/init/init.service';
import { CreatePreferenceComponent } from './components/create-preference/create-preference.component';
import { TimeAgoPipe } from './shared/pipes/time-ago/time-ago.pipe';
import { CustomerTicketsComponent } from './components/customer-tickets/customer-tickets.component';
import { VenueDetailsHeroComponent } from './components/venue-details-hero/venue-details-hero.component';
import { BadgesComponent } from './components/badges/badges.component';
import { EventDetailsHeroComponent } from './components/event-details-hero/event-details-hero.component';
import { LocationComponent } from './components/config/location/location.component';
import { InputComponent } from './shared/config/input/input.component';
import { AddressComponent } from './components/address/address.component';
import { ArtistDetailsHeroComponent } from './components/artist-details-hero/artist-details-hero.component';
import { PaymentSummaryComponent } from './components/payment-summary/payment-summary.component';
import { EventSummaryComponent } from './components/event-summary/event-summary.component';
import { SummaryDirective } from './directives/summary.directive';
import { HeroComponent } from './components/hero/hero.component';
import { InputDirective } from './directives/input.directive';
import { ResetPasswordComponent } from './components/pages/reset-password/reset-password.component';
import { PaginatorComponent } from './components/paginator/paginator.component';
import { AddReviewComponent } from './components/add-review/add-review.component';

const routerOptions: ExtraOptions = {
  anchorScrolling: 'enabled',
  scrollPositionRestoration: 'enabled' // Restores scroll position when navigating back
};

export function initApp(initService: InitService) {
  return () => initService.init();
}

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
    LocationDisplayComponent,
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
    ConfigItemNotFoundComponent,
    ConfigTextDirective,
    EditTextComponent,
    ListingApplicationsComponent,
    ArtistSummaryComponent,
    MyEventsComponent,
    MyEventComponent,
    ItemEventsComponent,
    ArtistEventsComponent,
    EventCheckoutComponent,
    CardDetailsComponent,
    MyTicketsComponent,
    ProfileMenuComponent,
    PaymentDetailsComponent,
    ListingApplicationCheckoutComponent,
    CustomerDashboardComponent,
    BreadcrumbComponent,
    VenueHomeComponent,
    ArtistHomeComponent,
    TimePickerComponent,
    DropDownComponent,
    CardComponent,
    VenueReviewsComponent,
    ArtistReviewsComponent,
    EventReviewsComponent,
    FooterComponent,
    ProfileDetailsComponent,
    MyProfileComponent,
    DashboardLayoutComponent,
    MyPreferenceComponent,
    SelectorComponent,
    SliderComponent,
    PreferenceDetailsComponent,
    EventCarouselComponent,
    BadgeComponent,
    PriceComponent,
    NumberComponent,
    CreatePreferenceComponent,
    TimeAgoPipe,
    CustomerTicketsComponent,
    VenueDetailsHeroComponent,
    BadgesComponent,
    EventDetailsHeroComponent,
    LocationComponent,
    InputComponent,
    AddressComponent,
    ArtistDetailsHeroComponent,
    PaymentSummaryComponent,
    EventSummaryComponent,
    SummaryDirective,
    HeroComponent,
    InputDirective,
    ResetPasswordComponent,
    PaginatorComponent,
    AddReviewComponent
  ],
  imports: [
    BrowserAnimationsModule, // Required for animations
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right', // Position of toast (top-right, bottom-right, etc.)
      timeOut: 7000, // Auto-dismiss after 3 seconds
      closeButton: true,
      progressBar: true
    }),
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    TextFieldModule,
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
    MatPaginatorModule,
    MatTimepickerModule,
    MatExpansionModule,
    MatDividerModule
],
  providers: [
    provideHttpClient(
      withInterceptorsFromDi()
    ),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    provideAnimationsAsync(),
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    {
      provide: APP_INITIALIZER,
      useFactory: initApp,
      deps: [InitService],
      multi: true
    }
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  bootstrap: [AppComponent]
})
export class AppModule { }
