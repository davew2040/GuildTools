import { async, ComponentFixture, TestBed } from './node_modules/@angular/core/testing';

import { FindPlayerDialogComponent } from './share-link-dialog.component';

describe('FindPlayerDialogComponent', () => {
  let component: FindPlayerDialogComponent;
  let fixture: ComponentFixture<FindPlayerDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FindPlayerDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FindPlayerDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
