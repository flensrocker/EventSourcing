import { Component } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'to-do-button-spinner',
  imports: [MatButtonModule, MatProgressSpinnerModule],
  template: `<mat-progress-spinner progressIndicator mode="indeterminate" diameter="20" aria-label="Loading" tabindex="" />`,
})
export class ToDoButtonSpinner {}
