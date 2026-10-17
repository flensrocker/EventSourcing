import { computed, signal } from '@angular/core';

export const busyTracker = () => {
  const busyCount = signal(0);

  const stop = () => busyCount.update((count) => (count > 0 ? count - 1 : 0));

  const start = () => {
    busyCount.update((count) => count + 1);
    return stop;
  };

  const isBusy = computed(() => busyCount() > 0);

  return {
    start,
    isBusy,
  };
};
