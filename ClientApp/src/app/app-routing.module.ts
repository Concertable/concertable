import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppComponent } from './app.component';
import { HomeComponent } from './pages/home/home.component';
import { VenueDashboardComponent } from './pages/venue-dashboard/venue-dashboard.component';
import { roleGuard } from './guards/role/role.guard';
import { Role } from './models/role';
import { ArtistDashboardComponent } from './pages/artist-dashboard/artist-dashboard.component';
import { ArtistFindComponent } from './pages/venue-find/artist-find.component';
import { VenueDetailsComponent } from './pages/venue-details/venue-details.component';
import { VenueFindComponent } from './components/venue-find/venue-find.component';
import { ArtistDetailsComponent } from './pages/artist-details/artist-details.component';
import { CustomerFindComponent } from './components/customer-find/customer-find.component';
import { RegisterComponent } from './pages/register/register.component';
import { MyVenueComponent } from './pages/my-venue/my-venue.component';
import { MyArtistComponent } from './pages/my-artist/my-artist.component';
import { CreateArtistComponent } from './pages/create-artist/create-artist.component';
import { CreateVenueComponent } from './pages/create-venue/create-venue.component';
import { EventDetailsComponent } from './pages/event-details/event-details.component';
import { ListingApplicationsComponent } from './pages/listing-applications/listing-applications.component';
import { MyEventsComponent } from './pages/my-events/my-events.component';
import { MyEventComponent } from './pages/my-event/my-event.component';
import { EventCheckoutComponent } from './pages/event-checkout/event-checkout.component';
import { MyTicketsComponent } from './pages/my-tickets/my-tickets.component';
import { PaymentDetailsComponent } from './components/payment-details/payment-details.component';
import { ListingApplicationCheckoutComponent } from './pages/listing-application-checkout/listing-application-checkout.component';

import { ListingApplicationResolver } from './resolvers/listing-application/listing-application.resolver';
import { EventResolver } from './resolvers/event/event.resolver';
import { MyEventResolver } from './resolvers/my-event/my-event.resolver';
import { VenueDetailsResolver } from './resolvers/venue-details/venue-details.resolver';
import { ArtistDetailsResolver } from './resolvers/artist-details/artist-details.resolver';
import { MyArtistResolver } from './resolvers/my-artist/my-artist.resolver';
import { MyVenueResolver } from './resolvers/my-venue/my-venue.resolver';
import { EventDetailsResolver } from './resolvers/event-details/event-details.resolver';
import { CustomerDashboardComponent } from './pages/customer-dashboard/customer-dashboard.component';
import { ArtistHomeComponent } from './pages/artist-home/artist-home.component';
import { VenueHomeComponent } from './pages/venue-home/venue-home.component';
import { ProfileMenuComponent } from './pages/profile-menu/profile-menu.component';
import { MyProfileComponent } from './pages/my-profile/my-profile.component';
import { MyProfileResolver } from './resolvers/my-profile/my-profile.resolver';
import { MyPreferenceComponent } from './components/my-preference/my-preference.component';
import { myPreferenceResolver } from './resolvers/my-preference/my-preference.resolver';
import { ListingApplicationsResolver } from './resolvers/listing-applications.resolver';
import { authGuard } from './guards/auth/auth.guard';
import { CreatePreferenceComponent } from './components/create-preference/create-preference.component';
import { CustomerTicketsComponent } from './components/customer-tickets/customer-tickets.component';
import { TicketViewType } from './models/ticket-view-type';
import { listingApplicationCheckoutGuard } from './guards/listing-application-checkout/listing-application-checkout.guard';
import { eventCheckoutGuard } from './guards/event-checkout/event-checkout.guard';
import { FindComponent } from './pages/find/find.component';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    data: { breadcrumb: 'Home' },
    children: [
      {
        path: '',
        component: CustomerDashboardComponent,
        data: { breadcrumb: 'Dashboard' }
      },
      {
        path: 'find',
        data: { breadcrumb: 'Find' },
        children: [
          { path: '', component: FindComponent, data: { breadcrumb: 'Search', headerType: 'event', context: 'customer' } },
          { path: 'venue/:id', component: VenueDetailsComponent, resolve: { venue: VenueDetailsResolver }, data: { breadcrumb: 'Venue Details' } },
          { path: 'artist/:id', component: ArtistDetailsComponent, resolve: { artist: ArtistDetailsResolver }, data: { breadcrumb: 'Artist Details' } },
          { path: 'event/:id', component: EventDetailsComponent, resolve: { event: EventDetailsResolver }, data: { breadcrumb: 'Event Details' } }
        ]
      },
      {
        path: 'event',
        data: { breadcrumb: 'Event' },
        children: [
          {
            path: 'checkout/:id',
            component: EventCheckoutComponent,
            resolve: { event: EventResolver },
            canActivate: [authGuard, roleGuard, eventCheckoutGuard],
            data: { breadcrumb: 'Checkout', role: 'Customer', }
          }
        ]
      },
      {
        path: 'application',
        canActivate: [roleGuard],
        data: { role: 'VenueManager', breadcrumb: 'Application' },
        children: [
          {
            path: 'checkout/:id',
            canActivate: [listingApplicationCheckoutGuard],
            component: ListingApplicationCheckoutComponent,
            resolve: { application: ListingApplicationResolver },
            data: { breadcrumb: 'Checkout' }
          }
        ]
      },
      {
        path: 'profile',
        component: ProfileMenuComponent,
        data: { breadcrumb: 'Profile' },
        children: [
          {
            path: 'preferences',
            data: { breadcrumb: 'Preferences' },
            children: [
              {
                path: '',
                component: MyPreferenceComponent,
                resolve: { preference: myPreferenceResolver },
                data: { breadcrumb: 'My Preferences' }
              },
              {
                path: 'create',
                component: CreatePreferenceComponent,
                data: { breadcrumb: 'Create Preference' }
              }
            ]
          },
          { path: 'my', component: MyProfileComponent, resolve: { user: MyProfileResolver }, data: { breadcrumb: 'My Profile' } },
          { path: 'payment', component: PaymentDetailsComponent, data: { breadcrumb: 'Payment' } },
          {
            path: 'tickets',
            data: { breadcrumb: 'Tickets' },
            children: [
              { path: '', component: MyTicketsComponent, data: { breadcrumb: 'All Tickets' } },
              { path: 'upcoming', component: CustomerTicketsComponent, data: { viewType: TicketViewType.Upcoming, breadcrumb: 'Upcoming Tickets' } },
              { path: 'history', component: CustomerTicketsComponent, data: { viewType: TicketViewType.History, breadcrumb: 'Past Tickets' } }
            ]
          }
        ]
      }
    ]
  },
  {
    path: 'venue',
    component: VenueHomeComponent,
    canActivate: [roleGuard],
    data: { role: 'VenueManager', breadcrumb: 'Venue' },
    children: [
      { path: '', component: VenueDashboardComponent, resolve: { venue: MyVenueResolver }, data: { breadcrumb: 'Dashboard' } },
      { path: 'my', component: MyVenueComponent, resolve: { venue: MyVenueResolver }, data: { breadcrumb: 'My Venue' } },
      {
        path: 'find',
        data: { breadcrumb: 'Find Artists' },
        children: [
          { path: '', component: FindComponent, data: { breadcrumb: 'Browse Artists', headerType: 'artist' } },
          { path: 'artist/:id', component: ArtistDetailsComponent, resolve: { artist: ArtistDetailsResolver }, data: { breadcrumb: 'Artist Details' } }
        ]
      },
      { path: 'create', component: CreateVenueComponent, data: { breadcrumb: 'Create Venue' } },
      {
        path: 'my',
        data: { breadcrumb: 'My' },
        children: [
          { path: 'events', component: MyEventsComponent, data: { breadcrumb: 'My Events' } },
          { path: 'events/event/:id', component: MyEventComponent, resolve: { event: MyEventResolver }, data: { breadcrumb: 'Event Details' } },
          { path: 'applications/:id', component: ListingApplicationsComponent, resolve: { applications: ListingApplicationsResolver }, data: { breadcrumb: 'Applications' } }
        ]
      }
    ]
  },
  {
    path: 'artist',
    component: ArtistHomeComponent,
    canActivate: [roleGuard],
    data: { role: 'ArtistManager', breadcrumb: 'Artist' },
    children: [
      { path: '', component: ArtistDashboardComponent, resolve: { artist: MyArtistResolver }, data: { breadcrumb: 'Dashboard' } },
      { path: 'my', component: MyArtistComponent, resolve: { artist: MyArtistResolver }, data: { breadcrumb: 'My Artist' } },
      {
        path: 'find',
        data: { breadcrumb: 'Find Venues' },
        children: [
          { path: '', component: FindComponent, data: { breadcrumb: 'Browse Venues', headerType: 'venue' } },
          { path: 'venue/:id', component: VenueDetailsComponent, resolve: { venue: VenueDetailsResolver }, data: { breadcrumb: 'Venue Details' } }
        ]
      },
      { path: 'create', component: CreateArtistComponent, data: { breadcrumb: 'Create Artist' } },
      {
        path: 'my',
        data: { breadcrumb: 'My' },
        children: [
          { path: 'events', component: MyEventsComponent, data: { breadcrumb: 'My Events' } },
          { path: 'events/event/:id', component: MyEventComponent, resolve: { event: MyEventResolver }, data: { breadcrumb: 'Event Details' } }
        ]
      }
    ]
  },
  { path: 'login', component: LoginComponent, data: { breadcrumb: 'Login' } },
  { path: 'register', component: RegisterComponent, data: { breadcrumb: 'Register' } },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
