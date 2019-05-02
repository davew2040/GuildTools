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
import { RegisterUserComponent } from './register-user/register-user.component';
import { LoginComponent } from './login/login.component';
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
    GuildStatsLauncherComponent,
    GuildProfileComponent,
    MyGuildProfilesComponent,
    FindGuildComponent,
    FindGuildDialogComponent,
    NewGuildProfileComponent,
    FindGuildComponent,
    BusyDirective,
    NewProfilePageComponent,
    ViewGuildProfileComponent,
    ViewMainComponent
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
  providers: [AuthService, BlizzardService, DataService, AccountService, BusyService, WowService, ErrorReportingService],
  bootstrap: [AppComponent],
  entryComponents: [FindGuildDialogComponent]
})
export class AppModule { }
