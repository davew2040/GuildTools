import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RaiderIoProfileStatsComponent } from './raider-io-profile-stats.component';

describe('RaiderIoProfileStatsComponent', () => {
  let component: RaiderIoProfileStatsComponent;
  let fixture: ComponentFixture<RaiderIoProfileStatsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RaiderIoProfileStatsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RaiderIoProfileStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
