import { Component, OnInit } from '@angular/core';
import { DataService } from '../services/data-services';
import { Router } from '@angular/router';
import { BlizzardRealms, BlizzardRegionDefinition } from '../data/blizzard-realms';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { SelectedGuild } from 'app/models/selected-guild';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { RoutePaths } from 'app/data/route-paths';

@Component({
  selector: 'app-guild-stats-launcher',
  templateUrl: './guild-stats-launcher.component.html',
  styleUrls: ['./guild-stats-launcher.component.css']
})
export class GuildStatsLauncherComponent implements OnInit {

  constructor(
    public dataService: DataService,
    public router: Router,
    private busyService: BusyService,
    private errorService: ErrorReportingService) {
  }

  ngOnInit() {
  }

  handleGuildSelected(guild: SelectedGuild) {
    this.router.navigate(['/' + RoutePaths.GuildStats, guild.region.Name, guild.name, guild.realm]);
  }
}
