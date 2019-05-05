import { Routes } from '@angular/router';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { RegisterUserComponent } from './components/account/register-user/register-user.component';
import { LoginComponent } from './components/account/login/login.component';
import { GuildStatsComponent } from './guild-stats/guild-stats.component';
import { GuildStatsLauncherComponent } from './guild-stats-launcher/guild-stats-launcher.component';
import { MyGuildProfilesComponent } from './components/my-guild-profiles/my-guild-profiles.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ResetPasswordWithTokenComponent } from './reset-password-token/reset-password-token.component';
import { NewProfilePageComponent } from './components/new-profile-page/new-profile-page.component';
import { RoutePaths } from './data/route-paths';
import { ViewGuildProfileComponent } from './components/view-guild-profile/view-guild-profile.component';
import { ConfirmRegistrationComponent } from './components/account/confirm-registration/confirm-registration.component';
import { ManagePermissionsComponent } from './components/manage-permissions/manage-permissions.component';
import { LandingPageComponent } from './components/landing-page/landing-page.component';

export const ROUTES: Routes = [
  { path: '', component: LandingPageComponent },
  { path: RoutePaths.UserSettings, component: UserSettingsComponent },
  { path: RoutePaths.Register, component: RegisterUserComponent },
  { path: RoutePaths.Login, component: LoginComponent },
  { path: RoutePaths.GuildStats, component: GuildStatsLauncherComponent },
  { path: RoutePaths.GuildStats + '/:region/:guild/:realm', component: GuildStatsComponent },
  { path: RoutePaths.MyProfiles, component: MyGuildProfilesComponent },
  { path: RoutePaths.NewProfile, component: NewProfilePageComponent },
  { path: RoutePaths.ResetPassword, component: ResetPasswordComponent },
  { path: RoutePaths.ResetPasswordWithToken, component: ResetPasswordWithTokenComponent },
  { path: RoutePaths.ConfirmEmail, component: ConfirmRegistrationComponent },
  { path: RoutePaths.ViewProfile + '/:id', component: ViewGuildProfileComponent},
  { path: RoutePaths.ManagePermissions + '/:id', component: ManagePermissionsComponent},
  { path: '**', redirectTo: '' }
];
