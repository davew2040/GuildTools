import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MobileMenuUnassignedPlayerComponent } from './mobile-menu-unassigned-player.component';

describe('MobileMenuUnassignedPlayerComponent', () => {
  let component: MobileMenuUnassignedPlayerComponent;
  let fixture: ComponentFixture<MobileMenuUnassignedPlayerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MobileMenuUnassignedPlayerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MobileMenuUnassignedPlayerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
