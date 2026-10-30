import { Component, computed, input } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { CommandHandler, errorToString } from '@flensrocker/event-sourcing-angular';

@Component({
  selector: 'to-do-command-error',
  imports: [MatButtonModule],
  template: `
    @if (errors(); as err) {
      <span>{{ err }}</span>
      <button type="button" matButton (click)="clearErrors()">Clear</button>
    }
  `,
})
export class ToDoCommandError {
  readonly commands = input.required<readonly CommandHandler[]>();

  protected readonly errors = computed(() =>
    this.commands()
      .map((c) => c.error())
      .filter((e) => e != null)
      .map((e) => errorToString(e))
      .join('; '),
  );

  protected clearErrors() {
    this.commands().forEach((c) => c.clearError());
  }
}
