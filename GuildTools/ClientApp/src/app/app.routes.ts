import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CallbackComponent } from './callback/callback.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { RegisterUserComponent } from './register-user/register-user.component';
import { LoginComponent } from './login/login.component';
import { GuildStatsComponent } from './guild-stats/guild-stats.component';
import { GuildStatsLauncherComponent } from './guild-stats-launcher/guild-stats-launcher.component';

export const ROUTES: Routes = [
  { path: '', component: GuildStatsLauncherComponent },
  { path: 'callback', component: CallbackComponent },
  { path: 'userSettings', component: UserSettingsComponent },
  { path: 'register', component: RegisterUserComponent },
  { path: 'login', component: LoginComponent },
  { path: 'guildstats', component: GuildStatsLauncherComponent },
  { path: 'guildstats/:guild/:realm', component: GuildStatsComponent },
  { path: '**', redirectTo: '' }
];
