import { Component, Inject } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material';
import { PlayerAlt } from 'app/services/ServiceTypes/service-types';

export interface IMobileMenuAltComponentData {
  callback: (action: Action, player: PlayerAlt) => void;
  alt: PlayerAlt;
}

export enum Action {
  PromoteToMain,
  Remove,
  ViewBlizzardProfile,
  ViewRaiderIo,
  ViewWowProgress
}

@Component({
  selector: 'app-mobile-menu-alt',
  templateUrl: './mobile-menu-alt.component.html',
  styleUrls: ['./mobile-menu-alt.component.scss']
})
export class MobileMenuAltComponent {
  constructor(private bottomSheetRef: MatBottomSheetRef<MobileMenuAltComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: IMobileMenuAltComponentData) {}

    public promoteToMain(event: MouseEvent): void {
      this.data.callback(Action.PromoteToMain, this.data.alt);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public remove(event: MouseEvent): void {
      this.data.callback(Action.Remove, this.data.alt);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public viewBlizzardProfile(event: MouseEvent): void {
      this.data.callback(Action.ViewBlizzardProfile, this.data.alt);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public viewRaiderIo(event: MouseEvent): void {
      this.data.callback(Action.ViewRaiderIo, this.data.alt);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }

    public viewWowProgress(event: MouseEvent): void {
      this.data.callback(Action.ViewWowProgress, this.data.alt);
      this.bottomSheetRef.dismiss();
      event.preventDefault();
    }
}
