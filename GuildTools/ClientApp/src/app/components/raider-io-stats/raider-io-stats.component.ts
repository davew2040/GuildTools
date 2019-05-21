import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { WowService } from 'app/services/wow-service';
import { RaiderIoStats } from 'app/services/ServiceTypes/service-types';
import { BlizzardService } from 'app/blizzard-services/blizzard-services';
import { DataService } from 'app/services/data-services';
import { NotificationRequestType } from 'app/data/notification-request-type';
import { RaiderIoStatsTableDefinition } from '../stats-table/subtypes/raider-io-stats-table-definition';
import { RaiderIoStatsPlayer } from '../stats-table/subtypes/raider-io-stats-player';


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
  tableDefinitions = new Array<RaiderIoStatsTableDefinition>();

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
    this.tableDefinitions = this.getStatsTableDefinitions();

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
    }
    return definition;
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
