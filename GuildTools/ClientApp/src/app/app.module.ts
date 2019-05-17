import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { ContextMenuModule } from 'ngx-contextmenu';

import { ROUTES } from './app.routes';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { AuthService } from './auth/auth.service';
import { CallbackComponent } from './callback/callback.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { RegisterUserComponent } from './components/account/register-user/register-user.component';
import { LoginComponent } from './components/account/login/login.component';
import { GuildStatsComponent } from './guild-stats/guild-stats.component';
import { BlizzardService } from './blizzard-services/blizzard-services';
import { DataService } from './services/data-services';
import { GuildStatsLauncherComponent } from './guild-stats-launcher/guild-stats-launcher.component';
import { MyGuildProfilesComponent } from './components/my-guild-profiles/my-guild-profiles.component';
import { GuildProfileComponent } from './guild-profile/guild-profile.component';
import { BusyService } from './shared-services/busy-service';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ResetPasswordWithTokenComponent } from './reset-password-token/reset-password-token.component';
import { AccountService } from './services/account-service';
import { FindGuildDialogComponent } from './dialogs/find-guild-dialog.component/find-guild-dialog.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FindGuildComponent } from './components/find-guild/find-guild.component';
import { FlexLayoutModule } from '@angular/flex-layout';
import { BusyDirective } from './directives/busy.directive';
import { MaterialModule } from './material.module';
import { NewGuildProfileComponent } from './components/new-guild-profile/new-guild-profile.component';
import { NewProfilePageComponent } from './components/new-profile-page/new-profile-page.component';
import { ViewGuildProfileComponent } from './components/view-guild-profile/view-guild-profile.component';
import { WowService } from './services/wow-service';
import { NgDragDropModule } from 'ng-drag-drop';
import { ViewMainComponent } from './components/view-main/view-main.component';
import { ErrorReportingService } from './shared-services/error-reporting-service';
import { FooterComponentComponent } from './components/footer-component/footer-component.component';
import { ConfirmRegistrationComponent } from './components/account/confirm-registration/confirm-registration.component';
import { NotificationService } from './shared-services/notification-service';
import { ManagePermissionsComponent } from './components/manage-permissions/manage-permissions.component';
import { StoredValuesService } from './shared-services/stored-values';
import { FindPlayerComponent } from './components/find-player/find-player.component';
import { FindPlayerDialogComponent } from './dialogs/find-player-dialog.component/find-player-dialog.component';
import { LandingPageComponent } from './components/landing-page/landing-page.component';
import { PlayerNotesComponentComponent } from './components/player-notes-component/player-notes-component.component';
import { ConfirmationDialogComponent } from './dialogs/confirmation-dialog.component/confirmation-dialog.component';
import { ShareLinkDialogComponent } from './dialogs/share-link-dialog.component/share-link-dialog.component';
import { RaiderIoStatsComponent } from './components/raider-io-stats/raider-io-stats.component';
import { GuildProfileStatsComponent } from './guild-profile-stats/guild-profile-stats.component';
import { StatsTableComponent } from './components/stats-table/stats-table.component';

@NgModule({
  declarations: [
    AppComponent,
    CallbackComponent,
    NavMenuComponent,
    UserSettingsComponent,
    RegisterUserComponent,
    LoginComponent,
    ResetPasswordComponent,
    ResetPasswordWithTokenComponent,
    GuildStatsComponent,
    GuildProfileStatsComponent,
    RaiderIoStatsComponent,
    GuildStatsLauncherComponent,
    GuildProfileComponent,
    MyGuildProfilesComponent,
    FindGuildComponent,
    FindGuildDialogComponent,
    NewGuildProfileComponent,
    FindGuildComponent,
    FindPlayerComponent,
    FindPlayerDialogComponent,
    BusyDirective,
    NewProfilePageComponent,
    ViewGuildProfileComponent,
    ViewMainComponent,
    FooterComponentComponent,
    ConfirmRegistrationComponent,
    ManagePermissionsComponent,
    LandingPageComponent,
    PlayerNotesComponentComponent,
    ConfirmationDialogComponent,
    ShareLinkDialogComponent,
    StatsTableComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(ROUTES),
    BrowserAnimationsModule,
    MaterialModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    ContextMenuModule.forRoot(),
    NgDragDropModule.forRoot()
  ],
  providers: [
    AuthService,
    BlizzardService,
    DataService,
    AccountService,
    WowService,
    ErrorReportingService,
    BusyService,
    NotificationService,
    StoredValuesService],
  bootstrap: [AppComponent],
  entryComponents: [FindGuildDialogComponent, FindPlayerDialogComponent, ConfirmationDialogComponent, ShareLinkDialogComponent]
})
export class AppModule { }
