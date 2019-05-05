import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerNotesComponentComponent } from './player-notes-component.component';

describe('PlayerNotesComponentComponent', () => {
  let component: PlayerNotesComponentComponent;
  let fixture: ComponentFixture<PlayerNotesComponentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlayerNotesComponentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlayerNotesComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
