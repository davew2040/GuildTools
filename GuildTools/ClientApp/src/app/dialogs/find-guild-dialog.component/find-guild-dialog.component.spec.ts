import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FindGuildDialogComponent } from './find-guild-dialog.component';

describe('FindGuildDialogComponentComponent', () => {
  let component: FindGuildDialogComponent;
  let fixture: ComponentFixture<FindGuildDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FindGuildDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FindGuildDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
