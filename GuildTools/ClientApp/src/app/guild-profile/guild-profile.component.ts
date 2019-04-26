import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router'
import { DataService } from '../services/data-services';
import { BlizzardService } from '../blizzard-services/blizzard-services';

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

  constructor(public route: ActivatedRoute, public dataService: DataService, public blizzardService: BlizzardService) { }

  ngOnInit() {
    this.route.params.subscribe(params => {

    });
  }
}
