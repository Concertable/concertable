import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppComponent } from './app.component';
import { HomeComponent } from './pages/home/home.component';
import { VenueDashboardComponent } from './pages/venue-dashboard/venue-dashboard.component';
import { roleGuard } from './guards/role/role.guard';
import { YourVenueComponent } from './pages/your-venue/your-venue.component';
import { Role } from './models/role';
import { ArtistDashboardComponent } from './pages/artist-dashboard/artist-dashboard.component';
import { ArtistFindComponent } from './pages/venue-find/artist-find.component';
import { VenueDetailsComponent } from './pages/venue-details/venue-details.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'venue', canActivate: [roleGuard], data: { role: Role.VenueManager }, component: VenueDashboardComponent,
    children: [
      { path: 'your-venue', component: YourVenueComponent },
    ] },
  { path: 'artist', canActivate: [roleGuard], data: { role: Role.ArtistManager }, component: ArtistDashboardComponent,
    children: [
      { path: 'find', component: ArtistFindComponent, },
      { path: 'find/venue', component: VenueDetailsComponent, },
    ] },  
  { path: '**', redirectTo: '', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
