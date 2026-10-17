import { Component, computed, inject, input, linkedSignal, Signal, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { finalize } from 'rxjs';

import { getEventSourceValue, IDomainEventVersionable } from '@flensrocker/event-sourcing-core';
import { TaskStatus, emptyTaskViewDetails } from '@flensrocker/to-do-core';

import { ToDoTaskClient } from './client';
import { toDoTaskViewDetailsStream } from './views';

// TODO extract & generalize helper

type IdVersion = Readonly<{
  id: string;
  version: number;
}>;

const versionTracker = <T extends IDomainEventVersionable>(id: Signal<string>, data: Signal<T | null | undefined>) => {
  const actualVersion = computed(() => data()?.Version ?? 0);

  const expectedVersion = linkedSignal<IdVersion, IdVersion>({
    source: () => ({ id: id(), version: actualVersion() }),
    computation: (source, previous) => {
      const prevId = previous?.value.id;
      const prevVersion = previous?.value.version ?? 0;

      if (prevId !== source.id) {
        return { ...source, version: 0 };
      }

      if (prevVersion < source.version) {
        return source;
      }

      return { ...source, version: prevVersion };
    },
  });

  const expectUpdate = computed(() => expectedVersion().version > actualVersion());

  const setExpectedVersion = (id: string, version: number) => expectedVersion.update((v) => (v.id === id ? { ...v, version } : v));

  return {
    expectUpdate,
    setExpectedVersion,
  };
};

const submitCounter = () => {
  const submitCount = signal(0);
  const stopSubmit = () => submitCount.update((c) => (c > 0 ? c - 1 : 0));

  return {
    start: () => {
      submitCount.update((c) => c + 1);
      return stopSubmit;
    },
    isSubmitting: computed(() => submitCount() > 0),
  };
};

@Component({
  selector: 'to-do-task-details',
  imports: [MatButtonModule, MatCardModule, MatProgressBarModule],
  template: `<mat-card>
    <mat-card-header>
      <mat-card-title>{{ title() }}</mat-card-title>
    </mat-card-header>
    <mat-card-content>
      @if (isLoading()) {
        <mat-progress-bar mode="indeterminate" />
      } @else if (isOpen()) {
        open
      } @else if (isCompleted()) {
        completed
      } @else if (isDismissed()) {
        dismissed with reason &quot;{{ dismissReason() }}&quot;
      }
    </mat-card-content>
    <mat-card-actions>
      <button type="button" matButton [disabled]="completeDisabled()" (click)="complete()">Complete</button>
      <button type="button" matButton [disabled]="dismissDisabled()" (click)="dismiss()">Dismiss</button>
      <button type="button" matButton [disabled]="reopenDisabled()" (click)="reopen()">Reopen</button>
    </mat-card-actions>
  </mat-card>`,
})
export class ToDoTaskDetails {
  readonly #client = inject(ToDoTaskClient);

  readonly taskId = input.required<string>();

  readonly #detailsStream = toDoTaskViewDetailsStream(this.taskId);
  readonly #details = computed(() => {
    const data = this.#detailsStream.value();
    const details = data == null ? emptyTaskViewDetails : getEventSourceValue(data);
    return details ?? emptyTaskViewDetails;
  });

  readonly #versionTracker = versionTracker(this.taskId, this.#details);
  readonly #submitCounter = submitCounter();

  protected readonly isLoading = computed(
    () => this.#detailsStream.isLoading() || this.#submitCounter.isSubmitting() || this.#versionTracker.expectUpdate(),
  );

  protected readonly title = computed(() => this.#details().Title);
  protected readonly status = computed(() => this.#details().Status);
  protected readonly dismissReason = computed(() => this.#details().DismissReason ?? '');

  protected readonly isOpen = computed(() => this.status() === TaskStatus.Open);
  protected readonly isCompleted = computed(() => this.status() === TaskStatus.Completed);
  protected readonly isDismissed = computed(() => this.status() === TaskStatus.Dismissed);

  protected readonly completeDisabled = computed(() => this.isLoading() || this.status() !== TaskStatus.Open);
  protected readonly dismissDisabled = computed(() => this.isLoading() || this.status() !== TaskStatus.Open);
  protected readonly reopenDisabled = computed(() => this.isLoading() || this.status() === TaskStatus.Open);

  protected complete() {
    const taskId = this.taskId();

    // TODO error handling
    const stopSubmit = this.#submitCounter.start();
    this.#client
      .complete(taskId)
      .pipe(finalize(stopSubmit))
      .subscribe({
        next: (response) => {
          this.#versionTracker.setExpectedVersion(taskId, response.Version);
        },
      });
  }

  protected dismiss() {
    const taskId = this.taskId();

    // TODO show dialog to enter reason
    const Reason = "I don't want to";

    // TODO error handling
    const stopSubmit = this.#submitCounter.start();
    this.#client
      .dismiss(taskId, { Reason })
      .pipe(finalize(stopSubmit))
      .subscribe({
        next: (response) => {
          this.#versionTracker.setExpectedVersion(taskId, response.Version);
        },
      });
  }

  protected reopen() {
    const taskId = this.taskId();

    // TODO error handling
    const stopSubmit = this.#submitCounter.start();
    this.#client
      .reopen(taskId)
      .pipe(finalize(stopSubmit))
      .subscribe({
        next: (response) => {
          this.#versionTracker.setExpectedVersion(taskId, response.Version);
        },
      });
  }
}
