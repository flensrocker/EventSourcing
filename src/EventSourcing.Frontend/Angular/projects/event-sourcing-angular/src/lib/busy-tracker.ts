import { computed, Signal, signal } from '@angular/core';

export type IsBusy = Readonly<{
  /**
   * Signals `true` if there are more `start` called than stopped.
   */
  isBusy: Signal<boolean>;
}>;

export type BusyTracker = IsBusy &
  Readonly<{
    /**
     * Use the returned callback to "stop" the busy-tracker.
     */
    start: () => () => void;
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

export const combinedIsBusy = (...busy: IsBusy[]): IsBusy => {
  return {
    isBusy: computed(() => {
      let someIsBusy = false;
      for (const b of busy) {
        // explicitly call every signal to track them all
        someIsBusy = b.isBusy() || someIsBusy;
      }
      return someIsBusy;
    }),
  };
};
