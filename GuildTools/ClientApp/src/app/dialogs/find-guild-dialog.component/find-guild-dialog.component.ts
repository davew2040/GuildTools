import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import { SelectedGuild } from '../../models/selected-guild';

@Component({
  selector: 'app-find-guild-dialog',
  templateUrl: './find-guild-dialog.component.html',
  styleUrls: ['./find-guild-dialog.component.css']
})
export class FindGuildDialogComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<FindGuildDialogComponent>) { }

  ngOnInit() {

  }

  handleChildCancelled() {
    this.dialogRef.close(false);
  }

  handleGuildSelected(guild: SelectedGuild) {
    this.dialogRef.close(guild);
  }
}
