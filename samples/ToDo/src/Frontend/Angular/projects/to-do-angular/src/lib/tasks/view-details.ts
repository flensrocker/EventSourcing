import { Component, computed, inject, input } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { EMPTY, Observable, switchMap } from 'rxjs';

import { IsBusy, combinedIsBusy, commandHandler, errorToString, versionTracker } from '@flensrocker/event-sourcing-angular';
import { getEventSourceValue } from '@flensrocker/event-sourcing-core';
import { TaskCommand_Dismiss_Response, TaskStatus, getTaskViewDetailsId } from '@flensrocker/to-do-core';

import { ToDoCommandButton } from '../to-do-command-button';
import { ToDoCommandError } from '../to-do-command-error';

import { ToDoTaskClient } from './client';
import { ToDoTaskDismissReasonDialog, ToDoTaskDismissReasonDialogResult } from './dismiss-reason-dialog';
import { toDoTaskViewDetailsStream } from './views';

@Component({
  selector: 'to-do-task-view-details',
  imports: [MatButtonModule, MatCardModule, MatProgressBarModule, ToDoCommandButton, ToDoCommandError],
  styles: `
    .error {
      color: var(--mat-sys-error);
    }

    .container {
      display: grid;
      grid-template-areas: 'content';
    }

    .loading {
      grid-area: content;
      justify-self: stretch;
      align-self: flex-end;
    }

    .content {
      grid-area: content;
      justify-self: start;
      align-self: center;
    }
  `,
  template: `<mat-card>
    <mat-card-header>
      <mat-card-title>{{ title() }}</mat-card-title>
    </mat-card-header>
    <mat-card-content class="container">
      @if (isLoading()) {
        <div class="loading">
          <mat-progress-bar mode="indeterminate" />
        </div>
      }

      <div class="content">
        @if (detailsError(); as errMsg) {
          <div class="error">ERROR: {{ errMsg }}</div>
        } @else {
          {{ statusText() }}
        }
      </div>
    </mat-card-content>
    <mat-card-actions>
      <to-do-command-button [command]="start">Start</to-do-command-button>
      <to-do-command-button [command]="pause">Pause</to-do-command-button>
      <to-do-command-button [command]="complete">Complete</to-do-command-button>
      <to-do-command-button [command]="dismiss">Dismiss</to-do-command-button>
      <to-do-command-button [command]="reopen">Reopen</to-do-command-button>
    </mat-card-actions>
    <mat-card-footer class="error">
      <to-do-command-error [commands]="[start, pause, complete, dismiss, reopen]" />
    </mat-card-footer>
  </mat-card>`,
})
export class ToDoTaskViewDetails {
  readonly #dialog = inject(MatDialog);
  readonly #client = inject(ToDoTaskClient);

  readonly taskId = input.required<string>();

  readonly #detailsStream = toDoTaskViewDetailsStream(this.taskId);
  readonly #details = computed(() => getEventSourceValue(this.#detailsStream.hasValue() ? this.#detailsStream.value() : undefined));
  protected readonly detailsError = computed(() => {
    const error = this.#detailsStream.error();
    return errorToString(error);
  });

  readonly #versionTracker = versionTracker(this.taskId, this.#details, getTaskViewDetailsId);

  readonly #status = computed(() => this.#details()?.Status);
  readonly #isOpened = computed(() => this.#status() === TaskStatus.Opened);
  readonly #isStarted = computed(() => this.#status() === TaskStatus.Started);
  readonly #isPaused = computed(() => this.#status() === TaskStatus.Paused);
  readonly #dismissReason = computed(() => this.#details()?.DismissReason);
  readonly #startedByUser = computed(() => this.#details()?.StartedByUserId); // TODO map to username

  protected readonly isLoading = computed(
    () => this.#detailsStream.error() == null && (this.#detailsStream.isLoading() || this.#versionTracker.updateExpected()),
  );

  protected readonly title = computed(() => this.#details()?.Title ?? '');
  protected readonly statusText = computed(() => {
    switch (this.#status()) {
      case TaskStatus.Completed:
        return 'completed';
      case TaskStatus.Dismissed:
        return `dismissed with reason "${this.#dismissReason()}"`;
      case TaskStatus.Opened:
        return 'opened';
      case TaskStatus.Paused:
        return 'paused';
      case TaskStatus.Started:
        return `started by ${this.#startedByUser()}`;
      default:
        return '';
    }
  });

  protected readonly start = commandHandler({
    // TODO show dialog to select user
    createCommand: () => [this.taskId(), this.#client.start(this.taskId(), { StartedAt: null, ByUserId: null })],
    disabledWhen: () => this.#someCommand.isBusy() || !this.#detailsStream.hasValue() || (!this.#isOpened() && !this.#isPaused()),
    versionTracker: this.#versionTracker,
  });
  protected readonly pause = commandHandler({
    createCommand: () => [this.taskId(), this.#client.pause(this.taskId(), { PausedAt: null })],
    disabledWhen: () => this.#someCommand.isBusy() || !this.#detailsStream.hasValue() || !this.#isStarted(),
    versionTracker: this.#versionTracker,
  });
  protected readonly complete = commandHandler({
    createCommand: () => [this.taskId(), this.#client.complete(this.taskId())],
    disabledWhen: () =>
      this.#someCommand.isBusy() ||
      !this.#detailsStream.hasValue() ||
      (this.#status() !== TaskStatus.Opened && !this.#isStarted() && !this.#isPaused()),
    versionTracker: this.#versionTracker,
  });
  protected readonly dismiss = commandHandler({
    createCommand: () => [this.taskId(), this.#dismiss(this.taskId())],
    disabledWhen: () =>
      this.#someCommand.isBusy() ||
      !this.#detailsStream.hasValue() ||
      (this.#status() !== TaskStatus.Opened && !this.#isStarted() && !this.#isPaused()),
    versionTracker: this.#versionTracker,
  });
  protected readonly reopen = commandHandler({
    createCommand: () => [this.taskId(), this.#client.reopen(this.taskId())],
    disabledWhen: () =>
      this.#someCommand.isBusy() || !this.#detailsStream.hasValue() || this.#isOpened() || this.#isStarted() || this.#isPaused(),
    versionTracker: this.#versionTracker,
  });

  readonly #someCommand: IsBusy;

  constructor() {
    this.#someCommand = combinedIsBusy(this.start, this.pause, this.complete, this.dismiss, this.reopen);
  }

  #dismiss(taskId: string): Observable<TaskCommand_Dismiss_Response> {
    return this.#dialog
      .open<ToDoTaskDismissReasonDialog, unknown, ToDoTaskDismissReasonDialogResult>(ToDoTaskDismissReasonDialog, {
        closeOnNavigation: true,
      })
      .afterClosed()
      .pipe(switchMap((result) => (result == null ? EMPTY : this.#client.dismiss(taskId, result))));
  }
}
