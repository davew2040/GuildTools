import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PlayerMain, BlizzardPlayer, PlayerAlt } from 'app/services/ServiceTypes/service-types';
import { WowService } from 'app/services/wow-service';

export interface RemoveMainEvent {
  main: PlayerMain;
}

export interface RemoveAltEvent {
  main: PlayerMain;
  alt: PlayerAlt;
}

@Component({
  selector: 'app-view-main',
  templateUrl: './view-main.component.html',
  styleUrls: ['./view-main.component.scss']
})
export class ViewMainComponent implements OnInit {

  @Input() playerMain: PlayerMain;

  @Output() mainRemoved = new EventEmitter<RemoveMainEvent>();
  @Output() altRemoved = new EventEmitter<RemoveAltEvent>();

  public isShowingActions = false;

  constructor(private wowService: WowService) { }

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
}
