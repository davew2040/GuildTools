import { MainSelectorDialogComponent } from './main-selector-dialog.component';
import { async, TestBed, ComponentFixture } from '@angular/core/testing';

describe('TextSelectorDialogComponent', () => {
  let component: MainSelectorDialogComponent;
  let fixture: ComponentFixture<MainSelectorDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MainSelectorDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainSelectorDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
