import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { ROUTES } from './app.routes';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { AuthService } from './auth/auth.service';
import { CallbackComponent } from './callback/callback.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { RegisterUserComponent } from './register-user/register-user.component';
import { LoginComponent } from './login/login.component';
import { GuildStatsComponent } from './guild-stats/guild-stats.component';
import { BlizzardService } from './blizzard-services/blizzard-services';
import { DataService } from './data-services/data-services';
import { GuildStatsLauncherComponent } from './guild-stats-launcher/guild-stats-launcher.component';
import { MyGuildProfilesComponent } from './my-guild-profiles/my-guild-profiles.component';
import { GuildProfileComponent } from './guild-profile/guild-profile.component';
import { BusyService } from './shared-services/busy-service';

@NgModule({
  declarations: [
    AppComponent,
    CallbackComponent,
    NavMenuComponent,
    HomeComponent,
    FetchDataComponent,
    UserSettingsComponent,
    RegisterUserComponent,
    LoginComponent,
    GuildStatsComponent,
    GuildStatsLauncherComponent,
    GuildProfileComponent,
    MyGuildProfilesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(ROUTES)
  ],
  providers: [AuthService, BlizzardService, DataService, BusyService],
  bootstrap: [AppComponent]
})
export class AppModule { }
