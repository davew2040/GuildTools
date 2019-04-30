import { BlizzardRegionDefinition } from '../data/blizzard-realms';
import { Injectable, Inject } from '@angular/core';

export enum WowClass {
   Warrior = 1,
   Paladin = 2,
   Hunter = 3,
   Rogue = 4,
   Priest = 5,
   DK = 6,
   Shaman = 7,
   Mage = 8,
   Warlock = 9,
   Monk = 10,
   Druid = 11,
   DH = 12
}

export class WowClassTags {
  private _tagMap: Map<WowClass, string>;

  constructor() {
    this._tagMap = new Map<WowClass, string>();

    this._tagMap[WowClass.Warrior] = 'warrior';
    this._tagMap[WowClass.Paladin] = 'paladin';
    this._tagMap[WowClass.Hunter] = 'hunter';
    this._tagMap[WowClass.Rogue] = 'rogue';
    this._tagMap[WowClass.Priest] = 'priest';
    this._tagMap[WowClass.DK] = 'dk';
    this._tagMap[WowClass.Shaman] = 'shaman';
    this._tagMap[WowClass.Mage] = 'mage';
    this._tagMap[WowClass.Warlock] = 'warlock';
    this._tagMap[WowClass.Monk] = 'monk';
    this._tagMap[WowClass.Druid] = 'druid';
    this._tagMap[WowClass.DH] = 'dh';
  }

  public getClassTag(targetClass: WowClass): string {
    return this._tagMap[targetClass];
  }
}

export class WowClassLabels {
  private _labelMap: Map<WowClass, string>;

  constructor() {
    this._labelMap = new Map<WowClass, string>();

    this._labelMap[WowClass.Warrior] = 'Warrior';
    this._labelMap[WowClass.Paladin] = 'Paladin';
    this._labelMap[WowClass.Hunter] = 'Hunter';
    this._labelMap[WowClass.Rogue] = 'Rogue';
    this._labelMap[WowClass.Priest] = 'Priest';
    this._labelMap[WowClass.DK] = 'Death Knight';
    this._labelMap[WowClass.Shaman] = 'Shaman';
    this._labelMap[WowClass.Mage] = 'Magemage';
    this._labelMap[WowClass.Warlock] = 'Warlock';
    this._labelMap[WowClass.Monk] = 'Monk';
    this._labelMap[WowClass.Druid] = 'Druid';
    this._labelMap[WowClass.DH] = 'Demon Hunter';
  }

  public getClassLabel(targetClass: WowClass): string {
    return this._labelMap[targetClass];
  }
}

@Injectable()
export class WowService {

  private wowClassTags: WowClassTags;
  private wowClassLabels: WowClassLabels;

  constructor(
    @Inject('BASE_URL') private baseUrl: string) {
      this.wowClassTags = new WowClassTags();
      this.wowClassLabels = new WowClassLabels();
  }

  public getClassTag(targetClass: WowClass): string {
    return this.wowClassTags.getClassTag(targetClass);
  }
  public getClassLabel(targetClass: WowClass): string {
    return this.wowClassLabels.getClassLabel(targetClass);
  }
}
