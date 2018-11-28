import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router'
import { DataService } from '../data-services/data-services';
import { BlizzardService } from '../blizzard-services/blizzard-services';
import { GuildProfile } from '../data-services/Models/GuildProfile';
import { GuildMember } from '../data-services/Models/GuildMember';

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
  guildMembers: GuildMember[] = [];
  guild: string;
  realm: string;
  region: string;
  prettyGuild: string;
  prettyRealm: string;
  guildExists: boolean = false;
  dataReady: boolean = false;
  statsTables: StatsTable[];

  constructor(public route: ActivatedRoute, public dataService: DataService, public blizzardService: BlizzardService) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.realm = params['realm'];
      this.guild = params['guild'];
      this.region = params['region'];
    });

    this.dataService.GetGuildExists(this.region, this.guild, this.realm, (result) => {
      if (result.Found === true) {
        this.prettyGuild = result.Name;
        this.prettyRealm = result.Realm;

        this.loadGuildStats(this.region, this.realm, this.guild);
      }
      else {
        this.pageStatus = GuildStatsStatus.GuildNotFound;
      }
    });
  }

  populateStatsTableDefinitions(): void {
    this.statsTables = [];

    let equippedIlvl = new StatsTable(this.guildMembers);
    equippedIlvl.Title = "Equipped ILVL";
    equippedIlvl.NameDisplayer = (m) => {
      return m.EquippedIlvl.toString();
    };
    equippedIlvl.Filter = (m => m.EquippedIlvl < 500 && m.EquippedIlvl > 0);
    equippedIlvl.Sorter = (g1, g2) => { return g2.EquippedIlvl - g1.EquippedIlvl; }
    this.statsTables.push(equippedIlvl);


    let maxIlvl = new StatsTable(this.guildMembers);
    maxIlvl.Title = "Max ILVL";
    maxIlvl.NameDisplayer = (m) => {
      return m.MaximumIlvl.toString();
    };
    maxIlvl.Filter = (m => m.MaximumIlvl < 500 && m.MaximumIlvl > 0);
    maxIlvl.Sorter = (g1, g2) => { return g2.MaximumIlvl - g1.MaximumIlvl; }

    this.statsTables.push(maxIlvl);


    let achieves = new StatsTable(this.guildMembers);
    achieves.Title = "Achievement Points";
    achieves.NameDisplayer = (m) => {
      return m.AchievementPoints.toString();
    };
    achieves.Sorter = (g1, g2) => { return g2.AchievementPoints - g1.AchievementPoints; }

    this.statsTables.push(achieves);


    let azerite = new StatsTable(this.guildMembers);
    azerite.Title = "Azerite Level";
    azerite.NameDisplayer = (m) => {
      return m.AzeriteLevel.toString();
    };
    azerite.Sorter = (g1, g2) => { return g2.AzeriteLevel - g1.AzeriteLevel; }
    azerite.Filter = (g) => g.AzeriteLevel > 0;

    this.statsTables.push(azerite);


    let mounts = new StatsTable(this.guildMembers);
    mounts.Title = "Mounts";
    mounts.NameDisplayer = (m) => {
      return m.MountCount.toString();
    };
    mounts.Sorter = (g1, g2) => { return g2.MountCount - g1.MountCount; }

    this.statsTables.push(mounts);


    let pets = new StatsTable(this.guildMembers);
    pets.Title = "Pets";
    pets.NameDisplayer = (m) => {
      return m.PetCount.toString();
    };
    pets.Sorter = (g1, g2) => { return g2.PetCount - g1.PetCount; }

    this.statsTables.push(pets);


    let arena2v2 = new StatsTable(this.guildMembers);
    arena2v2.Title = "Arena 2v2";
    arena2v2.NameDisplayer = (m) => {
      return m.Pvp2v2Rating.toString();
    };
    arena2v2.Sorter = (g1, g2) => { return g2.Pvp2v2Rating - g1.Pvp2v2Rating; }
    arena2v2.Filter = (g) => g.Pvp2v2Rating > 0;
    
    this.statsTables.push(arena2v2);


    let arena3v3 = new StatsTable(this.guildMembers);
    arena3v3.Title = "Arena 3v3";
    arena3v3.NameDisplayer = (m) => {
      return m.Pvp3v3Rating.toString();
    };
    arena3v3.Sorter = (g1, g2) => { return g2.Pvp3v3Rating - g1.Pvp3v3Rating; }
    arena3v3.Filter = (g) => g.Pvp3v3Rating > 0;

    this.statsTables.push(arena3v3);


    let rbg = new StatsTable(this.guildMembers);
    rbg.Title = "Rated BG's";
    rbg.NameDisplayer = (m) => {
      return m.PvpRbgRating.toString();
    };
    rbg.Sorter = (g1, g2) => { return g2.PvpRbgRating - g1.PvpRbgRating; }
    rbg.Filter = (g) => g.PvpRbgRating > 0;

    this.statsTables.push(rbg);


    let hk = new StatsTable(this.guildMembers);
    hk.Title = "Honorable Kills";
    hk.NameDisplayer = (m) => {
      return m.TotalHonorableKills.toString();
    };
    hk.Sorter = (g1, g2) => { return g2.TotalHonorableKills - g1.TotalHonorableKills; }
    hk.Filter = (g) => g.TotalHonorableKills > 0;

    this.statsTables.push(hk);


    let guildRank = new StatsTable(this.guildMembers);
    guildRank.Title = "Guild Rank";
    guildRank.NameDisplayer = (m) => {
      return m.GuildRank.toString();
    };
    guildRank.Sorter = (g1, g2) => { return g2.GuildRank - g1.GuildRank; }

    this.statsTables.push(guildRank);
  }

  loadGuildStats(region: string, realm: string, guild: string) {
    this.dataService.GetGuildMemberStats(
      region,
      guild,
      realm,
      (result) => {
        if (result && result.length > 0) {
          this.guildMembers = result;
          this.populateStatsTableDefinitions();
          this.pageStatus = GuildStatsStatus.Ready;
        }
        else {
          this.pageStatus = GuildStatsStatus.DataNotReady;
        }
      });
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

  getPlayerArmoryLink(player: GuildMember): string {
    let url = `https://worldofwarcraft.com/${this.getPlayerRegionUrlSegment(this.region)}/character/${BlizzardService.FormatRealm(player.Realm)}/${player.Name}`;
    return url;
  }

  getGuildArmoryLink(realm: string, guild: string): string {
    let url = `http://${this.region}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(realm)}/${guild}/`;
    return url;
  }

  getPlayerRegionUrlSegment(realm: string): string {
    if (realm.toLowerCase() == "us") {
      return "en-us";
    }
    else {
      return "en-gb";
    }
  }

  mapClassToColor(classIndex: number): string {
    switch (classIndex) {
      case 1:
        return "warrior";
      case 2:
        return "paladin";
      case 3:
        return "hunter";
      case 4:
        return "rogue";
      case 5:
        return "priest";
      case 6:
        return "dk";
      case 7:
        return "shaman";
      case 8:
        return "mage";
      case 9:
        return "warlock";
      case 10:
        return "monk";
      case 11:
        return "druid";
      case 12:
        return "dh";
      default:
        return "none";
    }
  }
}

class StatsTable {
  constructor(private members: GuildMember[]) {
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
  NameDisplayer: (g: GuildMember) => string;
  Sorter: (g1: GuildMember, g2: GuildMember) => number;
  Filter: (g: GuildMember) => boolean;
  SortedMembers(): GuildMember[] {
    return this.members
      .filter(g => this.Filter(g))
      .sort(this.Sorter)
      .slice(0, this.RowsDisplayed);
  };
  ShowMore(additional: number): void {
    this.RowsDisplayed += additional;
  }
}
