import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-new-profile-dialog',
  templateUrl: './new-profile-dialog.component.html',
  styleUrls: ['./new-profile-dialog.component.css']
})
export class NewProfileDialogComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<NewProfileDialogComponent>) { }

  closeDialog() {
    this.dialogRef.close('Pizza!');
  }

  ngOnInit() {
    
  }
}
