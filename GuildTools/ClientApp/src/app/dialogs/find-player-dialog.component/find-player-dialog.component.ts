import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import { SelectedPlayer } from 'app/models/selected-player';

@Component({
  selector: 'app-find-player-dialog',
  templateUrl: './find-player-dialog.component.html',
  styleUrls: ['./find-player-dialog.component.css']
})
export class FindPlayerDialogComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<FindPlayerDialogComponent>) { }

  ngOnInit() {

  }

  handleChildCancelled() {
    this.dialogRef.close(null);
  }

  handlePlayerSelected(player: SelectedPlayer) {
    this.dialogRef.close(player);
  }
}
