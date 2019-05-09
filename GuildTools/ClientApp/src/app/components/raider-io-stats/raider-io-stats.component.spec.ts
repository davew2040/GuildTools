import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RaiderIoStatsComponent } from './raider-io-stats.component';

describe('GuildStatsComponent', () => {
  let component: RaiderIoStatsComponent;
  let fixture: ComponentFixture<RaiderIoStatsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RaiderIoStatsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RaiderIoStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
