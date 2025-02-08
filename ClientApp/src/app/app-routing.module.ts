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

const routes: Routes = [
  { path: '', component: HomeComponent,
    children: [
      { path: 'find', component: CustomerFindComponent }
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
    ] },
  { path: 'artist', canActivate: [roleGuard], data: { role: "ArtistManager" }, component: ArtistDashboardComponent,
    children: [
      { path: 'my', component: MyArtistComponent },
      { path: 'find', component: ArtistFindComponent, },
      { path: 'find/venue', component: VenueDetailsComponent, },
      { path: 'create', component: CreateArtistComponent },
    ] },  
  { path: '**', redirectTo: '', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
