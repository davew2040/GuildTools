import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { PlayerMain } from 'app/services/ServiceTypes/service-types';

export interface IMainSelectorDialogData {
  items: Array<PlayerMain>;
}

@Component({
  selector: 'app-main-selector-dialog',
  templateUrl: './main-selector-dialog.component.html',
  styleUrls: ['./main-selector-dialog.component.css']
})
export class MainSelectorDialogComponent implements OnInit {
  public filteredPlayers = new Array<PlayerMain>();
  public selectedPlayer: PlayerMain = null;
  private filterText = '';
  private get selectedClassName() { return 'selected'; }

  constructor(
    public dialogRef: MatDialogRef<MainSelectorDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: IMainSelectorDialogData) {

  }

  ngOnInit() {
    this.filterPlayers();
  }

  public playersFilterChanged($e: any) {
    this.filterText = $e.target.value;
    this.filterPlayers();
  }

  public cancel() {
    this.dialogRef.close(null);
  }

  public okay() {
    this.dialogRef.close(this.selectedPlayer);
  }

  public selectPlayer(player: PlayerMain) {
    if (this.selectedPlayer === player) {
      this.selectedPlayer = null;
    }
    else {
      this.selectedPlayer = player;
    }
  }

  public getRowClasses(player: PlayerMain): Array<string> {
    const classes = new Array<string>();

    if (player === this.selectedPlayer){
      classes.push(this.selectedClassName);
    }

    return classes;
  }

  private filterPlayers(): void {
    const filterText = this.filterText.trim();
    if (filterText === '') {
      this.filteredPlayers = this.data.items;
    }
    else {
      this.filteredPlayers = this.data.items.filter(item => item.player.name.toLowerCase().includes(this.filterText.toLowerCase()));
    }
  }
}
