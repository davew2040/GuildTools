import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router';
import { DataService } from '../../services/data-services';
import { BlizzardService } from '../../blizzard-services/blizzard-services';
import {
  GuildMemberStats,
  FullGuildProfile,
  RaiderIoStatsResponse,
  AggregatedProfileRaiderIoStatsItem } from '../../services/ServiceTypes/service-types';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { BlizzardStatsPlayer } from 'app/components/stats-table/subtypes/blizzard-stats-player';
import { RaiderIoStatsTableDefinition } from 'app/components/stats-table/subtypes/raider-io-stats-table-definition';
import { RaiderIoStatsPlayer } from 'app/components/stats-table/subtypes/raider-io-stats-player';

enum GuildStatsStatus {
  Loading,
  ProfileNotFound,
  DataNotReady,
  Ready
}

@Component({
  selector: 'app-raider-io-profile-stats',
  templateUrl: './raider-io-profile-stats.component.html',
  styleUrls: ['./raider-io-profile-stats.component.css']
})
export class RaiderIoProfileStatsComponent implements OnInit {

  public pageStatus: GuildStatsStatus = GuildStatsStatus.Loading;
  public guildMembers: GuildMemberStats[] = [];
  public isCompleted = false;
  public profileId: number;
  public dataReady = false;
  public individualStatsResponses = new Array<RaiderIoStatsResponse>();
  public tableDefinitions = new Array<RaiderIoStatsTableDefinition>();
  public profile: FullGuildProfile;

  private timerHandler: any;

  private get recheckTime() { return 4000; }

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    public dataService: DataService,
    public blizzardService: BlizzardService,
    private busyService: BusyService,
    private errorService: ErrorReportingService) { }

  ngOnInit() {
    this.tableDefinitions = this.getStatsTableDefinitions();

    this.route.params.subscribe(params => {
      this.profileId = params['profileId'];
    });

    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        clearTimeout(this.timerHandler);
      }
    });

    this.loadProfileStats(this.profileId);
  }

  public get getStatsTablePlayers(): Array<RaiderIoStatsPlayer> {
    return this.guildMembers.map(x => new RaiderIoStatsPlayer(x));
  }

  getStatsTableDefinitions(): Array<RaiderIoStatsTableDefinition> {
    const statsTables = new Array<RaiderIoStatsTableDefinition>();

    statsTables.push(this.getOverallScoreDefinition());
    statsTables.push(this.getDpsScoreDefinition());
    statsTables.push(this.getTankScoreDefinition());
    statsTables.push(this.getHealerScoreDefinition());

    return statsTables;
  }

  getOverallScoreDefinition(): RaiderIoStatsTableDefinition {
    const definition = new RaiderIoStatsTableDefinition();
    definition.Title = 'Overall Score';
    definition.ValueGetter = (p) => {
      return (p as RaiderIoStatsPlayer).player.raiderIoOverall;
    };
    definition.Filter = (p) => {
      const casted = p as RaiderIoStatsPlayer;
      return casted.player.raiderIoOverall > 0;
    }
    return definition;
  }

  getDpsScoreDefinition(): RaiderIoStatsTableDefinition {
    const definition = new RaiderIoStatsTableDefinition();
    definition.Title = 'DPS Score';
    definition.ValueGetter = (p) => {
      return (p as RaiderIoStatsPlayer).player.raiderIoDps;
    };
    definition.Filter = (p) => {
      const casted = p as RaiderIoStatsPlayer;
      return casted.player.raiderIoDps > 0;
    }
    return definition;
  }

  getTankScoreDefinition(): RaiderIoStatsTableDefinition {
    const definition = new RaiderIoStatsTableDefinition();
    definition.Title = 'Tank Score';
    definition.ValueGetter = (p) => {
      return (p as RaiderIoStatsPlayer).player.raiderIoTank;
    };
    definition.Filter = (p) => {
      const casted = p as RaiderIoStatsPlayer;
      return casted.player.raiderIoTank > 0;
    }
    return definition;
  }

  getHealerScoreDefinition(): RaiderIoStatsTableDefinition {
    const definition = new RaiderIoStatsTableDefinition();
    definition.Title = 'Healer';
    definition.ValueGetter = (p) => {
      return (p as RaiderIoStatsPlayer).player.raiderIoHealer;
    };
    definition.Filter = (p) => {
      const casted = p as RaiderIoStatsPlayer;
      return casted.player.raiderIoHealer > 0;
    };
    return definition;
  }

  loadProfileStats(profileId: number) {
    this.busyService.setBusy();

    this.dataService.getRaiderIoProfileStats(profileId).subscribe(
      success => {
        this.busyService.unsetBusy();

        if (success.isCompleted) {
          this.isCompleted = true;
          this.guildMembers = success.values;
          this.pageStatus = GuildStatsStatus.Ready;
          this.profile = success.profile;
        }
        else {
          this.isCompleted = false;
          this.individualStatsResponses = success.individualGuildResponses;
          this.pageStatus = GuildStatsStatus.DataNotReady;
          this.timerHandler = setTimeout(
            () => {
              this.loadProfileStats(this.profileId);
            },
            this.recheckTime);
        }
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  public playerIsAlt(player: BlizzardStatsPlayer): boolean {
    if (!this.profile) {
      return false;
    }

    for (const main of this.profile.mains) {
      for (const alt of main.alts) {
        if (alt.player.realm.name.toLowerCase() === player.realmName.toLowerCase()
          && alt.player.name.toLowerCase() === player.name.toLowerCase()) {
            return true;
        }
      }
    }

    return false;
  }

  public getCompletionPercentage(percentage: number): string {
    return (percentage * 100).toFixed(0);
  }

  public getGuildName(statsResponse: AggregatedProfileRaiderIoStatsItem): string {
    return statsResponse.guildName;
  }

  public getPositionInLine(statsResponse: AggregatedProfileRaiderIoStatsItem): string {
    if (statsResponse.individualStats.isCompleted) {
      return 'Complete';
    }

    const position = statsResponse.individualStats.positionInQueue;

    if (position !== null && position !== undefined) {
      return statsResponse.individualStats.positionInQueue.toString();
    }

    return 'Active';
  }

  public getPercentComplete(statsResponse: AggregatedProfileRaiderIoStatsItem): string {
    if (statsResponse.individualStats.isCompleted) {
      return '100';
    }

    return this.getCompletionPercentage(statsResponse.individualStats.completionProgress);
  }

  pageLoading(): boolean {
    return this.pageStatus === GuildStatsStatus.Loading;
  }

  profileNotFound(): boolean {
    return this.pageStatus === GuildStatsStatus.ProfileNotFound;
  }

  dataNotReady(): boolean {
    return this.pageStatus === GuildStatsStatus.DataNotReady;
  }

  pageReady(): boolean {
    return this.pageStatus === GuildStatsStatus.Ready;
  }

  getGuildArmoryLink(player: GuildMemberStats): string {
    const url = `http://${player.regionName}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(player.realmName)}`
      + `/${BlizzardService.FormatGuild(player.guildName)}/`;
    return url;
  }

  getPlayerRegionUrlSegment(realm: string): string {
    if (realm.toLowerCase() === "us") {
      return "en-us";
    }
    else {
      return "en-gb";
    }
  }
}
