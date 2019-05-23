import { Component, OnInit, Inject } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material';
import { PlayerMain } from 'app/services/ServiceTypes/service-types';

export interface IMobileMenuMainComponentData {
  callback: (action: Action, player: PlayerMain) => void;
  main: PlayerMain;
}

export enum Action {
  RemoveMain,
  ViewBlizzardProfile,
  ViewRaiderIo,
  ViewWowProgress
}

@Component({
  selector: 'app-mobile-menu-main',
  templateUrl: './mobile-menu-main.component.html',
  styleUrls: ['./mobile-menu-main.component.scss']
})
export class MobileMenuMainComponent {
  constructor(private bottomSheetRef: MatBottomSheetRef<MobileMenuMainComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: IMobileMenuMainComponentData) {}

    public removeMain(event: MouseEvent): void {
      this.data.callback(Action.RemoveMain, this.data.main);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public viewBlizzardProfile(event: MouseEvent): void {
      this.data.callback(Action.ViewBlizzardProfile, this.data.main);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public viewRaiderIo(event: MouseEvent): void {
      this.data.callback(Action.ViewRaiderIo, this.data.main);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public viewWowProgress(event: MouseEvent): void {
      this.data.callback(Action.ViewWowProgress, this.data.main);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }
}
