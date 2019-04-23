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
  selector: 'app-guild-profile',
  templateUrl: './guild-profile.component.html',
  styleUrls: ['./guild-profile.component.css']
})
export class GuildProfileComponent implements OnInit {

  constructor(public profileId: number, public route: ActivatedRoute, public dataService: DataService, public blizzardService: BlizzardService) { }

  ngOnInit() {
    this.route.params.subscribe(params => {

    });
  }
}
