import { finalize, Observable } from 'rxjs';

import { CommandResult } from '@flensrocker/event-sourcing-core';

import { BusyTracker } from './busy-tracker';
import { VersionTracker } from './version-tracker';

export type CommandHandler = Readonly<{
  submit: <T extends CommandResult>(command: Observable<T>, id?: string) => void;
}>;

export const commandHandler = (busyTracker: BusyTracker, versionTracker?: VersionTracker): CommandHandler => {
  const submit = <T extends CommandResult>(command: Observable<T>, id?: string) => {
    const stopSubmit = busyTracker.start();

    command.pipe(finalize(stopSubmit)).subscribe({
      next: (response) => {
        if (id != null && id !== '') {
          versionTracker?.setExpectedVersion(id, response.Version);
        }
      },
      error: (error) => {
        // TODO error handling
        console.error('Error while submitting command', error);
      },
    });
  };

  return {
    submit,
  };
};
