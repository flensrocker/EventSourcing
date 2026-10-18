import { computed, Signal, signal } from '@angular/core';

export type BusyTracker = Readonly<{
  /**
   * Use the returned callback to "stop" the busy-tracker.
   */
  start: () => () => void;
  /**
   * Signals `true` if there are more `start` called than stopped.
   */
  isBusy: Signal<boolean>;
}>;

export const busyTracker = (): BusyTracker => {
  let nextId = 1;
  const busyIds = signal<readonly number[]>([]);

  const start = () => {
    const busyId = nextId++;
    busyIds.update((count) => [...count, busyId]);

    return () => busyIds.update((ids) => ids.filter((id) => id !== busyId));
  };

  const isBusy = computed(() => busyIds().length > 0);

  return {
    start,
    isBusy,
  };
};
