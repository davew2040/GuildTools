import { Component, OnInit } from '@angular/core';
import { BlizzardRegionDefinition, BlizzardRealms } from '../../data/blizzard-realms';

@Component({
  selector: 'app-find-guild',
  templateUrl: './find-guild.component.html',
  styleUrls: ['./find-guild.component.css']
})
export class FindGuildComponent implements OnInit {

  public selectedRegionName: string;
  public regions: Array<BlizzardRegionDefinition>;

  constructor() { 
    this.regions = BlizzardRealms.AllRealms;
    this.selectedRegionName = this.regions[0].Name;
  }

  ngOnInit() {
  }

}
