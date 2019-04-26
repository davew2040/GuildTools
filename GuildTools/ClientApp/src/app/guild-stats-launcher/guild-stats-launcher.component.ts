import { Component, OnInit } from '@angular/core';
import { DataService } from '../services/data-services';
import { Router } from '@angular/router';
import { BlizzardRealms, BlizzardRegionDefinition } from '../data/blizzard-realms';

@Component({
  selector: 'app-guild-stats-launcher',
  templateUrl: './guild-stats-launcher.component.html',
  styleUrls: ['./guild-stats-launcher.component.css']
})
export class GuildStatsLauncherComponent implements OnInit {

  guildNotFound = false;
  private savedRealmKey = 'stats-launcher-realm';
  private savedGuildKey = 'stats-launcher-guild';
  private savedRegionKey = 'stats-launcher-region';
  savedRealm: string;
  savedGuild: string;
  savedRegion: string;
  selectedRegionName: string;
  regions: Array<BlizzardRegionDefinition>;

  constructor(public dataService: DataService, public router: Router) {
  }

  ngOnInit() {
    this.regions = BlizzardRealms.AllRealms;

    this.savedRealm = localStorage.getItem(this.savedRealmKey);
    this.savedGuild = localStorage.getItem(this.savedGuildKey);
    this.savedRegion = localStorage.getItem(this.savedRegionKey);

    this.selectedRegionName = this.savedRegion || this.regions[0].Name;
  }

  search(region: string, guild: string, realm: string): void {
    this.dataService.getGuildExists(region, guild, realm)
      .subscribe(
        success => {
          if (success.found) {
            this.guildNotFound = false;

            localStorage.setItem(this.savedRealmKey, realm);
            localStorage.setItem(this.savedGuildKey, guild);
            localStorage.setItem(this.savedRegionKey, region);

            this.router.navigate(['/guildstats', region, guild, realm]);
          }
          else {
            this.guildNotFound = true;
          }
        },
        error => {
          console.error(error)
        });
   }
}
