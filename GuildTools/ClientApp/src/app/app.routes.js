"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var callback_component_1 = require("./callback/callback.component");
var user_settings_component_1 = require("./user-settings/user-settings.component");
var register_user_component_1 = require("./register-user/register-user.component");
var login_component_1 = require("./login/login.component");
var guild_stats_component_1 = require("./guild-stats/guild-stats.component");
var guild_stats_launcher_component_1 = require("./guild-stats-launcher/guild-stats-launcher.component");
var my_guild_profiles_component_1 = require("./my-guild-profiles/my-guild-profiles.component");
exports.ROUTES = [
    { path: '', component: guild_stats_launcher_component_1.GuildStatsLauncherComponent },
    { path: 'callback', component: callback_component_1.CallbackComponent },
    { path: 'userSettings', component: user_settings_component_1.UserSettingsComponent },
    { path: 'register', component: register_user_component_1.RegisterUserComponent },
    { path: 'login', component: login_component_1.LoginComponent },
    { path: 'guildstats', component: guild_stats_launcher_component_1.GuildStatsLauncherComponent },
    { path: 'guildstats/:region/:guild/:realm', component: guild_stats_component_1.GuildStatsComponent },
    { path: 'myprofiles', component: my_guild_profiles_component_1.MyGuildProfilesComponent },
    { path: '**', redirectTo: '' }
];
//# sourceMappingURL=app.routes.js.map