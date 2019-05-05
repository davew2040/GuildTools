import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { PlayerMain, BlizzardPlayer, PlayerAlt } from 'app/services/ServiceTypes/service-types';
import { WowService } from 'app/services/wow-service';
import { DataService } from 'app/services/data-services';
import { ContextMenuComponent } from 'ngx-contextmenu';

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

  constructor(private wowService: WowService, private dataService: DataService) { }

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

  public onMouseEnter(event: any): void {
    const targetDiv = event.currentTarget.querySelector('.player-actions-col div');
    targetDiv.classList.remove('hidden');
  }

  public onMouseLeave(event: any): void {
    const targetDiv = event.currentTarget.querySelector('.player-actions-col div');
    targetDiv.classList.add('hidden');
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
}
