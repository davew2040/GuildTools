import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { NgDragDropModule } from 'ng-drag-drop';
import { ContextMenuModule } from 'ngx-contextmenu';
import { AppComponent } from './app.component';
import { ROUTES } from './app.routes';
import { AuthService } from './auth/auth.service';
import { BlizzardService } from './blizzard-services/blizzard-services';
import { CallbackComponent } from './callback/callback.component';
import { ConfirmRegistrationComponent } from './components/account/confirm-registration/confirm-registration.component';
import { LoginComponent } from './components/account/login/login.component';
import { RegisterUserComponent } from './components/account/register-user/register-user.component';
import { FindGuildComponent } from './components/find-guild/find-guild.component';
import { FindPlayerComponent } from './components/find-player/find-player.component';
import { FooterComponentComponent } from './components/footer-component/footer-component.component';
import { GuildProfileStatsComponent } from './components/guild-profile-stats/guild-profile-stats.component';
import { GuildStatsLauncherComponent } from './components/guild-stats-launcher/guild-stats-launcher.component';
import { GuildStatsComponent } from './components/guild-stats/guild-stats.component';
import { LandingPageComponent } from './components/landing-page/landing-page.component';
import { ManagePermissionsComponent } from './components/manage-permissions/manage-permissions.component';
import { MyGuildProfilesComponent } from './components/my-guild-profiles/my-guild-profiles.component';
import { NewGuildProfileComponent } from './components/new-guild-profile/new-guild-profile.component';
import { NewProfilePageComponent } from './components/new-profile-page/new-profile-page.component';
import { PlayerNotesComponentComponent } from './components/player-notes-component/player-notes-component.component';
import { RaiderIoProfileStatsComponent } from './components/raider-io-profile-stats/raider-io-profile-stats.component';
import { RaiderIoStatsComponent } from './components/raider-io-stats/raider-io-stats.component';
import { StatsTableComponent } from './components/stats-table/stats-table.component';
import { MobileMenuMainComponent } from './components/view-guild-profile/mobile-menu-main/mobile-menu-main.component';
import { ViewGuildProfileComponent } from './components/view-guild-profile/view-guild-profile.component';
import { MobileMenuAltComponent } from './components/view-main/mobile-menu-alt/mobile-menu-alt.component';
import { ViewMainComponent } from './components/view-main/view-main.component';
import { ConfirmationDialogComponent } from './dialogs/confirmation-dialog.component/confirmation-dialog.component';
import { FindGuildDialogComponent } from './dialogs/find-guild-dialog.component/find-guild-dialog.component';
import { FindPlayerDialogComponent } from './dialogs/find-player-dialog.component/find-player-dialog.component';
import { ShareLinkDialogComponent } from './dialogs/share-link-dialog.component/share-link-dialog.component';
import { BusyDirective } from './directives/busy.directive';
import { LongPressDirective } from './directives/long-press.directive';
import { MaterialModule } from './material.module';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ResetPasswordWithTokenComponent } from './components/account/reset-password-token/reset-password-token.component';
import { ResetPasswordComponent } from './components/account/reset-password/reset-password.component';
import { AccountService } from './services/account-service';
import { DataService } from './services/data-services';
import { UtilitiesService } from './services/utilities-service';
import { WowService } from './services/wow-service';
import { BusyService } from './shared-services/busy-service';
import { ErrorReportingService } from './shared-services/error-reporting-service';
import { NotificationService } from './shared-services/notification-service';
import { StoredValuesService } from './shared-services/stored-values';
import { MainSelectorDialogComponent } from './dialogs/main-selector-dialog.component/main-selector-dialog.component';
import { MobileMenuUnassignedPlayerComponent } from './components/view-guild-profile/mobile-menu-unassigned-player/mobile-menu-unassigned-player.component';
import { LongTouchDirective } from './directives/long-touch.directive';
import { UserSettingsComponent } from './components/account/user-settings/user-settings.component';
import { NavService } from './services/nav-service';


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
    RaiderIoProfileStatsComponent,
    RaiderIoStatsComponent,
    GuildStatsLauncherComponent,
    MyGuildProfilesComponent,
    FindGuildComponent,
    FindGuildDialogComponent,
    NewGuildProfileComponent,
    FindGuildComponent,
    FindPlayerComponent,
    FindPlayerDialogComponent,
    BusyDirective,
    LongPressDirective,
    LongTouchDirective,
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
    StatsTableComponent,
    MainSelectorDialogComponent,
    MobileMenuMainComponent,
    MobileMenuAltComponent,
    MobileMenuUnassignedPlayerComponent
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
    StoredValuesService,
    UtilitiesService,
    NavService],
  bootstrap: [AppComponent],
  entryComponents: [
    FindGuildDialogComponent,
    FindPlayerDialogComponent,
    ConfirmationDialogComponent,
    ShareLinkDialogComponent,
    MainSelectorDialogComponent,
    MobileMenuMainComponent,
    MobileMenuAltComponent,
    MobileMenuUnassignedPlayerComponent]
})
export class AppModule { }
