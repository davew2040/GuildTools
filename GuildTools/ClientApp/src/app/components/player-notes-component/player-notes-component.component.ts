import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-player-notes-component',
  templateUrl: './player-notes-component.component.html',
  styleUrls: ['./player-notes-component.component.scss']
})
export class PlayerNotesComponentComponent implements OnInit {

  @Input() public text: string;
  @Input() public isAdmin = false;
  @Output() public notesChanged = new EventEmitter<string>();

  public isEditing = false;
  public editingText: string;

  constructor() { }

  ngOnInit() {
    this.editingText = this.text;
  }

  public startEditing(): void {
    this.isEditing = true;
  }

  public cancelEditing(): void {
    this.isEditing = false;
    this.editingText = this.text;
  }

  public saveEdits(): void {
    this.isEditing = false;
    if (this.editingText === this.text) {
      return;
    }
    this.text = this.editingText;
    this.notesChanged.emit(this.text);
  }
}
