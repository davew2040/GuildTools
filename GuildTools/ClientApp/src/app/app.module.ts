import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { ROUTES } from './app.routes';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { AuthService } from './auth/auth.service';
import { CallbackComponent } from './callback/callback.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { RegisterUserComponent } from './register-user/register-user.component';
import { LoginComponent } from './login/login.component';
import { GuildStatsComponent } from './guild-stats/guild-stats.component';
import { BlizzardService } from './blizzard-services/blizzard-services';
import { DataService } from './services/data-services';
import { GuildStatsLauncherComponent } from './guild-stats-launcher/guild-stats-launcher.component';
import { MyGuildProfilesComponent } from './my-guild-profiles/my-guild-profiles.component';
import { GuildProfileComponent } from './guild-profile/guild-profile.component';
import { BusyService } from './shared-services/busy-service';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ResetPasswordWithTokenComponent } from './reset-password-token/reset-password-token.component';
import { AccountService } from './services/account-service';
import { NewProfileDialogComponent } from './dialogs/new-profile-dialog-component/new-profile-dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialog, MatDialogModule, MatInputModule, MatSelectModule, MatGridListModule } from '@angular/material';
import { OverlayModule } from "@angular/cdk/overlay";
import { FindGuildComponent } from './components/find-guild/find-guild.component';

@NgModule({
  declarations: [
    AppComponent,
    CallbackComponent,
    NavMenuComponent,
    HomeComponent,
    UserSettingsComponent,
    RegisterUserComponent,
    LoginComponent,
    ResetPasswordComponent,
    ResetPasswordWithTokenComponent,
    GuildStatsComponent,
    GuildStatsLauncherComponent,
    GuildProfileComponent,
    MyGuildProfilesComponent,
    NewProfileDialogComponent,
    FindGuildComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(ROUTES),
    BrowserAnimationsModule,
    OverlayModule,
    MatDialogModule,
    MatInputModule,
    MatSelectModule,
    MatGridListModule
  ],
  providers: [AuthService, BlizzardService, DataService, AccountService, BusyService, MatDialog],
  bootstrap: [AppComponent],
  entryComponents: [NewProfileDialogComponent]
})
export class AppModule { }
