import { Component, OnInit, Input, Output, EventEmitter, ViewChild, Testability } from '@angular/core';
import { PlayerMain, PlayerAlt } from 'app/services/ServiceTypes/service-types';
import { WowService } from 'app/services/wow-service';
import { DataService } from 'app/services/data-services';
import { ContextMenuComponent } from 'ngx-contextmenu';
import { UtilitiesService } from 'app/services/utilities-service';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { Action, IMobileMenuAltComponentData, MobileMenuAltComponent } from './mobile-menu-alt/mobile-menu-alt.component';

export interface RemoveMainEvent {
  main: PlayerMain;
}

export interface RemoveAltEvent {
  main: PlayerMain;
  alt: PlayerAlt;
}

export interface PromoteAltToMainEvent {
  main: PlayerMain;
  alt: PlayerAlt;
}

export interface EditPlayerNotesEvent {
  main: PlayerMain;
  newNotes: string;
}

export interface EditOfficerNotesEvent {
  main: PlayerMain;
  newNotes: string;
}

@Component({
  selector: 'app-view-main',
  templateUrl: './view-main.component.html',
  styleUrls: ['./view-main.component.scss']
})
export class ViewMainComponent implements OnInit {

  @Input() playerMain: PlayerMain;
  @Input() isAdmin = false;

  @Output() public mainRemoved = new EventEmitter<RemoveMainEvent>();
  @Output() public altRemoved = new EventEmitter<RemoveAltEvent>();
  @Output() public altPromoted = new EventEmitter<PromoteAltToMainEvent>();
  @Output() public playerNotesChanged = new EventEmitter<EditPlayerNotesEvent>();
  @Output() public officerNotesChanged = new EventEmitter<EditOfficerNotesEvent>();

  public isShowingActions = false;

  @ViewChild(ContextMenuComponent) public contextMenu: ContextMenuComponent;

  public contextMenuActions = [
    {
      html: (item) => `Promote to Main`,
      click: (item) => this.promoteToMain(item),
      visible: (item) => this.isAdmin,
      enabled: (item) => true
    },
    {
      html: (item) => `Delete`,
      click: (item) => this.removeAlt(this.playerMain, item),
      visible: (item) => this.isAdmin,
      enabled: (item) => true
    },
    {
      divider: true,
      visible: (item) => this.isAdmin,
      enabled: (item) => true
    },
    {
      html: (item) => `View Blizzard Profile`,
      click: (item) => {
        console.log('viewing...');
        this.viewBlizzardProfile(item);
      },
      visible: (item) => true,
      enabled: (item) => true
    },
    {
      html: (item) => `View Raider.IO`,
      click: (item) => this.viewRaiderIo(item),
      visible: (item) => true,
      enabled: (item) => true
    },
    {
      html: (item) => `View WoW Progress`,
      click: (item) => this.viewWowProgress(item),
      visible: (item) => true,
      enabled: (item) => true
    }
  ];

  constructor(
    private wowService: WowService,
    private dataService: DataService,
    public utilitiesService: UtilitiesService,
    private bottomSheet: MatBottomSheet) { }

  ngOnInit() {
  }

  public getRowClass(alt: PlayerAlt): string {
    return `background-${this.wowService.getClassTag(alt.player.class)}`;
  }

  public getAltDescription(alt: PlayerAlt): string {
    return `Level ${alt.player.level} ${this.wowService.getClassLabel(alt.player.class)}`;
  }

  public get showActions(): boolean {
    return this.isShowingActions;
  }

  public openMobileMenu_Alt(alt: PlayerAlt) {
    this.bottomSheet.open(MobileMenuAltComponent, {
      data: {
        callback: this.handleBottomSheetResult,
        alt: alt
      } as IMobileMenuAltComponentData
    });
  }

  private handleBottomSheetResult = (action: Action, player: PlayerAlt): void => {
    if (action === Action.PromoteToMain) {
      this.promoteToMain(player);
    }
    else if (action === Action.Remove) {
      this.removeAlt(this.playerMain, player);
    }
    else if (action === Action.ViewBlizzardProfile) {
      this.viewBlizzardProfile(player);
    }
    else if (action === Action.ViewRaiderIo) {
      this.viewRaiderIo(player);
    }
    else if (action === Action.ViewWowProgress) {
      this.viewWowProgress(player);
    }
  }

  public removeAlt(main: PlayerMain, alt: PlayerAlt) {
    const event: RemoveAltEvent = {
      main: main,
      alt: alt
    };

    this.altRemoved.emit(event);
  }

  public removeMain(main: PlayerMain) {
    const event: RemoveMainEvent = {
      main: main
    };

    this.mainRemoved.emit(event);
  }

  public promoteToMain(alt: PlayerAlt) {
    this.altPromoted.emit({
      alt: alt,
      main: this.playerMain
    } as PromoteAltToMainEvent);
  }

  public viewBlizzardProfile(main: PlayerMain) {
    WowService.viewBlizzardProfile(main.player);
  }

  public viewRaiderIo(main: PlayerMain) {
    WowService.viewRaiderIo(main.player);
  }

  public viewWowProgress(main: PlayerMain) {
    WowService.viewWowProgress(main.player);
  }

  public onPlayerNotesChanged(newNotes: string) {
    this.playerMain.notes = newNotes;

    this.playerNotesChanged.emit({
      main: this.playerMain,
      newNotes: newNotes
    } as EditPlayerNotesEvent);
  }

  public onOfficerNotesChanged(newNotes: string) {
    this.playerMain.officerNotes = newNotes;

    this.officerNotesChanged.emit({
      main: this.playerMain,
      newNotes: newNotes
    } as EditOfficerNotesEvent);
  }

  public get isMobile() {
    return this.utilitiesService.mobileService.AnyMobile();
  }
}
