import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { WowService } from 'app/services/wow-service';
import { RaiderIoStats } from 'app/services/ServiceTypes/service-types';
import { BlizzardService } from 'app/blizzard-services/blizzard-services';
import { DataService } from 'app/services/data-services';
import { NotificationRequestType } from 'app/data/notification-request-type';


enum GuildStatsStatus {
  Loading,
  GuildNotFound,
  DataNotReady,
  Ready
}

@Component({
  selector: 'app-raider-io-stats',
  templateUrl: './raider-io-stats.component.html',
  styleUrls: ['./raider-io-stats.component.css']
})
export class RaiderIoStatsComponent implements OnInit {

  pageStatus: GuildStatsStatus = GuildStatsStatus.Loading;
  guildMembers: RaiderIoStats[] = [];
  isCompleted = false;
  completionProgress = 0.0;
  placeInLine: number;
  guild: string;
  realm: string;
  region: string;
  prettyGuild: string;
  prettyRealm: string;
  guildExists = false;
  dataReady = false;
  statsTables: StatsTable[];

  public notificationEmailAddress = '';
  public emailDisabled = false;

  private timerHandler: any;

  private get recheckTimer() { return 4000; }

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    public dataService: DataService,
    private busyService: BusyService,
    private wowService: WowService,
    private errorService: ErrorReportingService) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.realm = params['realm'];
      this.guild = params['guild'];
      this.region = params['region'];
    });

    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        clearTimeout(this.timerHandler);
      }
    });

    this.busyService.setBusy();

    this.dataService.getGuildExists(this.region, this.guild, this.realm).subscribe(
      success => {
        this.busyService.unsetBusy();

        if (success !== null) {
          if (success.found === true) {
            this.prettyGuild = success.guildName;
            this.prettyRealm = success.realmName;

            this.loadGuildStats(this.region, this.realm, this.guild, true);
          }
          else {
            this.pageStatus = GuildStatsStatus.GuildNotFound;
          }
        }
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  populateStatsTableDefinitions(): void {
    this.statsTables = [];

    const overallLevel = new StatsTable(this.guildMembers);
    overallLevel.Title = 'Overall Score';
    overallLevel.NameDisplayer = (m) => {
      return m.raiderIoOverall.toString();
    };
    overallLevel.Filter = (m => m.raiderIoOverall > 0);
    overallLevel.Sorter = (g1, g2) => g2.raiderIoOverall - g1.raiderIoOverall;
    this.statsTables.push(overallLevel);

    const dpsLevel = new StatsTable(this.guildMembers);
    dpsLevel.Title = 'DPS Score';
    dpsLevel.NameDisplayer = (m) => {
      return m.raiderIoDps.toString();
    };
    dpsLevel.Filter = (m => m.raiderIoDps > 0);
    dpsLevel.Sorter = (g1, g2) => g2.raiderIoDps - g1.raiderIoDps;
    this.statsTables.push(dpsLevel);

    const tankLevel = new StatsTable(this.guildMembers);
    tankLevel.Title = 'Tank Score';
    tankLevel.NameDisplayer = (m) => {
      return m.raiderIoTank.toString();
    };
    tankLevel.Filter = (m => m.raiderIoTank > 0);
    tankLevel.Sorter = (g1, g2) => g2.raiderIoTank - g1.raiderIoTank;
    this.statsTables.push(tankLevel);

    const healerLevel = new StatsTable(this.guildMembers);
    healerLevel.Title = 'Healer Score';
    healerLevel.NameDisplayer = (m) => {
      return m.raiderIoHealer.toString();
    };
    healerLevel.Filter = (m => m.raiderIoHealer > 0);
    healerLevel.Sorter = (g1, g2) => g2.raiderIoHealer - g1.raiderIoHealer;
    this.statsTables.push(healerLevel);
  }

  loadGuildStats(region: string, realm: string, guild: string, showBusy: boolean) {
    if (showBusy) {
      this.busyService.setBusy();
    }

    this.dataService.getRaiderIoStats(region, guild, realm).subscribe(
      success => {
        if (showBusy) {
          this.busyService.unsetBusy();
        }
        if (success.isCompleted) {
          this.guildMembers = success.values;
          this.populateStatsTableDefinitions();
          this.pageStatus = GuildStatsStatus.Ready;
        }
        else {
          this.completionProgress = success.completionProgress;
          this.placeInLine = success.positionInQueue;
          this.pageStatus = GuildStatsStatus.DataNotReady;
          this.timerHandler = setTimeout(() => {
            this.loadGuildStats(region, realm, guild, false);
          },
          this.recheckTimer);
        }
      },
      error => {
        if (showBusy) {
          this.busyService.unsetBusy();
        }

        this.errorService.reportApiError(error);
      });
  }

  public requestNotification() {
    const email = this.notificationEmailAddress;

    this.busyService.setBusy();
    this.dataService.requestStatsCompleteNotification(
        email,
        this.region,
        this.guild,
        this.realm,
        NotificationRequestType.RaiderIoStatsRequestComplete)
      .subscribe(
        success => {
          this.emailDisabled = true;
          this.busyService.unsetBusy();
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
        }
      )
  }

  public get inLine(): boolean {
    return this.placeInLine !== undefined && this.placeInLine !== null;
  }

  getCompletionPercentage(percentage: number): string {
    return (percentage * 100).toFixed(0);
  }

  pageLoading(): boolean {
    return this.pageStatus === GuildStatsStatus.Loading;
  }

  guildNotFound(): boolean {
    return this.pageStatus === GuildStatsStatus.GuildNotFound;
  }

  dataNotReady(): boolean {
    return this.pageStatus === GuildStatsStatus.DataNotReady;
  }

  pageReady(): boolean {
    return this.pageStatus === GuildStatsStatus.Ready;
  }

  getPlayerArmoryLink(player: RaiderIoStats): string {
    let url = `https://worldofwarcraft.com/${this.getPlayerRegionUrlSegment(this.region)}`
      + `character/${this.region.toLowerCase()}/${BlizzardService.FormatRealm(player.realm)}/${player.name}`;
    return url;
  }

  getGuildArmoryLink(realm: string, guild: string): string {
    let url = `http://${this.region}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(realm)}/${guild}/`;
    return url;
  }
s
  getPlayerRegionUrlSegment(realm: string): string {
    if (realm.toLowerCase() === "us") {
      return "en-us";
    }
    else {
      return "en-gb";
    }
  }

  mapClassToColor(classIndex: number): string {
    return 'background-' + this.wowService.getClassTag(classIndex);
  }
}

class StatsTable {
  constructor(private members: RaiderIoStats[]) {
    this.RowsDisplayed = 10;
    this.Sorter = (g1, g2) => {
      if (g1 === g2) {
        return 0;
      }
      else if (g2 > g1) {
        return 1;
      }
      else {
        return 0;
      }
    };
    this.Filter = (g) => true;
  }

  RowsDisplayed: number;
  Title: string;
  NameDisplayer: (g: RaiderIoStats) => string;
  Sorter: (g1: RaiderIoStats, g2: RaiderIoStats) => number;
  Filter: (g: RaiderIoStats) => boolean;
  SortedMembers(): RaiderIoStats[] {
    return this.members
      .filter(g => this.Filter(g))
      .sort(this.Sorter)
      .slice(0, this.RowsDisplayed);
  };
  ShowMore(additional: number): void {
    this.RowsDisplayed += additional;
  }
}
