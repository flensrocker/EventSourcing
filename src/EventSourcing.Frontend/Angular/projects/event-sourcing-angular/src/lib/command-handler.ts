import { computed, signal, Signal } from '@angular/core';

import { finalize, isObservable, Observable } from 'rxjs';

import { CommandResult } from '@flensrocker/event-sourcing-core';

import { busyTracker } from './busy-tracker';
import { VersionTracker } from './version-tracker';

export type CommandHandlerConfig<T extends CommandResult> = Readonly<{
  createCommand: () => Observable<T> | [string, Observable<T>];
  disabledWhen?: () => boolean;
  versionTracker?: VersionTracker;
}>;

export type CommandHandler = Readonly<{
  submit: () => void;
  disabled: Signal<boolean>;
  isBusy: Signal<boolean>;
  error: Signal<unknown>;
  clearError: () => void;
}>;

/**
 * When calling `submit` the `createCommand` function is executed to get the next command.
 * The provided observable will be subscribed to and therefore executed.
 * @param createCommand A function that returns the observable to execute the command, optional with the associated id for the version-tracker. Return `EMPTY` for a no-op.
 * @param disabledWhen An optional function that returns true, if the command can NOT be executed. Will be wrapped in a `computed` and observes the returned `isBusy`.
 * @param versionTracker An optional version-tracker, if the command is associated with an id.
 * @returns An easy to use object for a button in a component.
 * ```html
 * <button type="button" [disabled]="bar.disabled()" (click)="bar.submit()">...</button>
 * @if (bar.error(); as barErr) {
 *   Error: {{ barErr }}
 * }
 * ```
 *
 * ```ts
 * @Component({...})
 * export class Foo {
 *   readonly fooId = input.required<string>();
 *   ...
 *
 *   protected readonly bar = commandHandler({
 *     createCommand: () => [this.fooId(), this.#client.bar(this.fooId())],
 *     disabledWhen: () => this.isLoading(),
 *     versionTracker: this.#versionTracker,
 *   });
 * }
 * ```
 */
export const commandHandler = <T extends CommandResult>(config: CommandHandlerConfig<T>): CommandHandler => {
  const busy = busyTracker();

  const error = signal<unknown>(undefined);
  const clearError = () => error.set(undefined);

  const submit = () => {
    const cmd = config.createCommand();
    const [id, command] = isObservable(cmd) ? [undefined, cmd] : cmd;

    const stopSubmit = busy.start();
    clearError();

    command.pipe(finalize(() => stopSubmit?.())).subscribe({
      next: (response) => {
        if (id != null && id !== '') {
          config.versionTracker?.setExpectedVersion(id, response.Version);
        }
      },
      error: error.set,
    });
  };

  return {
    submit,
    disabled: computed(() => busy.isBusy() || (config.disabledWhen?.() ?? false)),
    isBusy: busy.isBusy,
    error: error.asReadonly(),
    clearError,
  };
};
