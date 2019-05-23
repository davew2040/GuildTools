import { Component, OnInit, Inject } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA, MatDialog } from '@angular/material';
import { PlayerMain, StoredPlayer } from 'app/services/ServiceTypes/service-types';
import { MainSelectorDialogComponent, IMainSelectorDialogData } from 'app/dialogs/main-selector-dialog.component/main-selector-dialog.component';

export enum UnassignedPlayerMobileMenuAction {
  AddAsMain,
  AssignToMain
}

export interface IMobileMenuUnassignedPlayerData {
  callback: (action: UnassignedPlayerMobileMenuAction, player: StoredPlayer, main: PlayerMain) => void;
  player: StoredPlayer;
  allMains: Array<PlayerMain>;
}

@Component({
  selector: 'app-mobile-menu-unassigned-player',
  templateUrl: './mobile-menu-unassigned-player.component.html',
  styleUrls: ['./mobile-menu-unassigned-player.component.scss']
})
export class MobileMenuUnassignedPlayerComponent {
  constructor(
    private bottomSheetRef: MatBottomSheetRef<MobileMenuUnassignedPlayerComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: IMobileMenuUnassignedPlayerData,
    private dialog: MatDialog) {}

    public assignToMain(): void {
      const dialogRef = this.dialog.open(MainSelectorDialogComponent, {
          data: {
            items: this.data.allMains
          } as IMainSelectorDialogData
        });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.data.callback(UnassignedPlayerMobileMenuAction.AssignToMain, this.data.player, result);
        }

        this.bottomSheetRef.dismiss();
      });
    }

    public addAsMain(): void {
      this.data.callback(UnassignedPlayerMobileMenuAction.AddAsMain, this.data.player, null);

      this.bottomSheetRef.dismiss();
    }
}
