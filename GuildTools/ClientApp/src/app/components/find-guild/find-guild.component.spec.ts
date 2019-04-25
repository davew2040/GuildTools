import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FindGuildComponent } from './find-guild.component';

describe('FindGuildComponent', () => {
  let component: FindGuildComponent;
  let fixture: ComponentFixture<FindGuildComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FindGuildComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FindGuildComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
