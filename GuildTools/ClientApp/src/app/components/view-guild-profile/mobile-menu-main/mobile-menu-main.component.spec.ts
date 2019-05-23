import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MobileMenuMainComponent } from './mobile-menu-main.component';

describe('MobileMenuMainComponent', () => {
  let component: MobileMenuMainComponent;
  let fixture: ComponentFixture<MobileMenuMainComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MobileMenuMainComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MobileMenuMainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
