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
import { EventResolver } from './resolvers/event/event.resolver';
import { MyTicketsComponent } from './pages/my-tickets/my-tickets.component';
import { ProfileDetailsComponent } from './pages/profile-details/profile-details.component';
import { PaymentDetailsComponent } from './components/payment-details/payment-details.component';
import { ListingApplicationCheckoutComponent } from './pages/listing-application-checkout/listing-application-checkout.component';
import { ListingApplicationResolver } from './resolvers/listing-application/listing-application.resolver';

const routes: Routes = [
  { path: '', component: HomeComponent,
    children: [
      { path: 'find', component: CustomerFindComponent },
      { path: 'find/venue', component: VenueDetailsComponent },
      { path: 'find/artist', component: ArtistDetailsComponent },
      { path: 'find/event', component: EventDetailsComponent },
      { path: 'event/checkout/:id', component: EventCheckoutComponent, resolve: { event: EventResolver} },
      { path: 'application/checkout/:id', component: ListingApplicationCheckoutComponent, resolve: { listingApplication: ListingApplicationResolver} },
      { path: 'profile', component: ProfileDetailsComponent, children: [
        { path: 'payment', component: PaymentDetailsComponent }
      ] },
      { path: 'profile/tickets', component: MyTicketsComponent }
    ]
   },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'venue', canActivate: [roleGuard], data: { role: "VenueManager" }, component: VenueDashboardComponent,
    children: [
      { path: 'my', component: MyVenueComponent },
      { path: 'find', component: VenueFindComponent },
      { path: 'find/artist', component: ArtistDetailsComponent },
      { path: 'create', component: CreateVenueComponent },
      { path: 'my/events', component: MyEventsComponent },
      { path: 'my/events/event', component: MyEventComponent },
      { path: 'my/applications', component: ListingApplicationsComponent },
    ] },
  { path: 'artist', canActivate: [roleGuard], data: { role: "ArtistManager" }, component: ArtistDashboardComponent,
    children: [
      { path: 'my', component: MyArtistComponent },
      { path: 'find', component: ArtistFindComponent, },
      { path: 'find/venue', component: VenueDetailsComponent, },
      { path: 'create', component: CreateArtistComponent },
    ] },
  

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
