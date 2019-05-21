import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router'
import { DataService } from '../../services/data-services';
import { BlizzardService } from '../../blizzard-services/blizzard-services';
import { GuildMemberStats } from '../../services/ServiceTypes/service-types';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { WowService } from 'app/services/wow-service';
import { NotificationRequestType } from 'app/data/notification-request-type';
import { StatsTableDefinition } from 'app/components/stats-table/subtypes/stats-table-definition';
import { BlizzardStatsPlayer } from 'app/components/stats-table/subtypes/blizzard-stats-player';
import { BlizzardPlayerStatsTableDefinition } from 'app/components/stats-table/subtypes/blizzard-player-stats-table-definition';

enum GuildStatsStatus {
  Loading,
  GuildNotFound,
  DataNotReady,
  Ready
}

@Component({
  selector: 'app-guild-stats',
  templateUrl: './guild-stats.component.html',
  styleUrls: ['./guild-stats.component.css']
})
export class GuildStatsComponent implements OnInit {

  pageStatus: GuildStatsStatus = GuildStatsStatus.Loading;
  guildMembers: GuildMemberStats[] = [];
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
  tableDefinitions = new Array<BlizzardPlayerStatsTableDefinition>();

  public notificationEmailAddress = '';
  public emailDisabled = false;

  private timerHandler: any;

  private get recheckTime() { return 4000; }

  constructor(
      public route: ActivatedRoute,
      public router: Router,
      public dataService: DataService,
      public blizzardService: BlizzardService,
      private busyService: BusyService,
      private wowService: WowService,
      private errorService: ErrorReportingService) {
  }

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

            this.loadGuildStats(this.region, this.realm, this.guild);
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

  public get getStatsTablePlayers(): Array<BlizzardStatsPlayer> {
    return this.guildMembers.map(x => new BlizzardStatsPlayer(x));
  }

  getStatsTableDefinitions(): Array<BlizzardPlayerStatsTableDefinition> {
    const statsTables = new Array<BlizzardPlayerStatsTableDefinition>();

    statsTables.push(this.getEquippedIlvlDefinition());
    statsTables.push(this.getMaxIlvlDefinition());
    statsTables.push(this.getAchievementsDefinition());
    statsTables.push(this.getAzeriteLevelDefinition());
    statsTables.push(this.getMountsDefinition());
    statsTables.push(this.getPetsDefinition());
    statsTables.push(this.getArena2v2Definition());
    statsTables.push(this.getArena3v3Definition());
    statsTables.push(this.getRbgDefinition());
    statsTables.push(this.getHonorKillsDefinition());

    return statsTables;
  }

  getEquippedIlvlDefinition(): BlizzardPlayerStatsTableDefinition {
    const equippedIlvl = new BlizzardPlayerStatsTableDefinition();
    equippedIlvl.Title = 'Equipped ILVL';
    equippedIlvl.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.equippedIlvl;
    };
    equippedIlvl.Filter = (p) => {
      const casted = p as BlizzardStatsPlayer;
      return casted.player.equippedIlvl < 500 && casted.player.equippedIlvl > 0;
    }
    return equippedIlvl;
  }

  getMaxIlvlDefinition(): BlizzardPlayerStatsTableDefinition {
    const maxIlvl = new BlizzardPlayerStatsTableDefinition();
    maxIlvl.Title = 'Max ILVL';
    maxIlvl.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.maximumIlvl;
    };
    maxIlvl.Filter = (p) => {
      const casted = p as BlizzardStatsPlayer;
      return casted.player.maximumIlvl < 500 && casted.player.maximumIlvl > 0;
    };

    return maxIlvl;
  }

  getAchievementsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Achievement Points';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.achievementPoints;
    };

    return definition;
  }

  getAzeriteLevelDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Azerite Level';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.azeriteLevel;
    };
    definition.Filter = p => (p as BlizzardStatsPlayer).player.azeriteLevel > 0;

    return definition;
  }

  getMountsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Mounts';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.mountCount;
    };

    return definition;
  }

  getPetsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Pets';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.petCount;
    };

    return definition;
  }

  getArena2v2Definition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Arena 2v2';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.pvp2v2Rating;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.pvp2v2Rating > 0;

    return definition;
  }

  getArena3v3Definition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Arena 3v3';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.pvp3v3Rating;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.pvp3v3Rating > 0;

    return definition;
  }

  getRbgDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Rated BG\'s';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.pvpRbgRating;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.pvpRbgRating > 0;

    return definition;
  }

  getHonorKillsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Honorable Kills';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.totalHonorableKills;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.totalHonorableKills > 0;

    return definition;
  }

  loadGuildStats(region: string, realm: string, guild: string) {
    this.busyService.setBusy();

    this.dataService.getBlizzardGuildMemberStats(region, guild, realm).subscribe(
      success => {
        this.busyService.unsetBusy();
        if (success.isCompleted) {
          this.guildMembers = success.values;
          this.pageStatus = GuildStatsStatus.Ready;
        }
        else {
          this.completionProgress = success.completionProgress;
          this.placeInLine = success.positionInQueue;
          this.pageStatus = GuildStatsStatus.DataNotReady;
          this.timerHandler = setTimeout(
            () => {
              this.loadGuildStats(region, realm, guild);
            },
            this.recheckTime);
        }
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  public requestNotification() {
    const email = this.notificationEmailAddress;

    this.busyService.setBusy();
    this.dataService.requestStatsCompleteNotification(
        email, this.region, this.guild, this.realm, NotificationRequestType.StatsRequestComplete)
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

  getGuildArmoryLinkForPlayer(player: BlizzardStatsPlayer): string {
    let url = `http://${player.regionName}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(player.realmName)}/${player.guildName}/`;
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

  mapClassToColor(classIndex: number): string {
    return 'background-' + this.wowService.getClassTag(classIndex);
  }
}
