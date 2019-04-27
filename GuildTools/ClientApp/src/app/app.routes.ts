import { Routes } from '@angular/router';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { RegisterUserComponent } from './register-user/register-user.component';
import { LoginComponent } from './login/login.component';
import { GuildStatsComponent } from './guild-stats/guild-stats.component';
import { GuildStatsLauncherComponent } from './guild-stats-launcher/guild-stats-launcher.component';
import { MyGuildProfilesComponent } from './components/my-guild-profiles/my-guild-profiles.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ResetPasswordWithTokenComponent } from './reset-password-token/reset-password-token.component';
import { NewProfilePageComponent } from './components/new-profile-page/new-profile-page.component';
import { RoutePaths } from './data/route-paths';
import { ViewGuildProfileComponent } from './components/view-guild-profile/view-guild-profile.component';

export const ROUTES: Routes = [
  { path: '', component: GuildStatsLauncherComponent },
  { path: RoutePaths.UserSettings, component: UserSettingsComponent },
  { path: RoutePaths.Register, component: RegisterUserComponent },
  { path: RoutePaths.Login, component: LoginComponent },
  { path: RoutePaths.GuildStats, component: GuildStatsLauncherComponent },
  { path: RoutePaths.GuildStats + '/:region/:guild/:realm', component: GuildStatsComponent },
  { path: RoutePaths.MyProfiles, component: MyGuildProfilesComponent },
  { path: RoutePaths.NewProfile, component: NewProfilePageComponent },
  { path: RoutePaths.ResetPassword, component: ResetPasswordComponent },
  { path: RoutePaths.ResetPasswordWithToken, component: ResetPasswordWithTokenComponent },
  { path: RoutePaths.ViewProfile + '/:id', component: ViewGuildProfileComponent},
  { path: '**', redirectTo: '' }
];
