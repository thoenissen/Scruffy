import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RaidSetupComponent } from './raid-setup.component';

describe('RaidSetupComponent', () => {
  let component: RaidSetupComponent;
  let fixture: ComponentFixture<RaidSetupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RaidSetupComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RaidSetupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
