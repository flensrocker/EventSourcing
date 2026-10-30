import { Component, input } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { CommandHandler } from '@flensrocker/event-sourcing-angular';

import { ToDoButtonSpinner } from './to-do-button-spinner';

@Component({
  selector: 'to-do-command-button',
  imports: [MatButtonModule, ToDoButtonSpinner],
  template: `
    @let cmd = command();
    <button type="button" matButton [disabled]="cmd.disabled()" [showProgress]="cmd.isBusy()" (click)="cmd.submit()">
      <ng-content />
      <to-do-button-spinner progressIndicator />
    </button>
  `,
})
export class ToDoCommandButton {
  readonly command = input.required<CommandHandler>();
}
