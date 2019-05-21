import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GuildProfileStatsComponent } from './guild-profile-stats.component';

describe('GuildProfileStatsComponent', () => {
  let component: GuildProfileStatsComponent;
  let fixture: ComponentFixture<GuildProfileStatsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GuildProfileStatsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GuildProfileStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
