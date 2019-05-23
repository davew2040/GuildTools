import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MobileMenuAltComponent } from './mobile-menu-alt.component';

describe('MobileMenuAltComponent', () => {
  let component: MobileMenuAltComponent;
  let fixture: ComponentFixture<MobileMenuAltComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MobileMenuAltComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MobileMenuAltComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
