import { computed, linkedSignal, Signal } from '@angular/core';

import { IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

type IdVersion = Readonly<{
  id: string;
  version: number;
}>;

export type VersionTracker = Readonly<{
  updateExpected: Signal<boolean>;
  setExpectedVersion: (id: string, version: number) => void;
}>;

export const versionTracker = <T extends IDomainEventVersionable>(
  id: Signal<string>,
  data: Signal<T | null | undefined>,
): VersionTracker => {
  const dataVersion = computed(() => data()?.Version ?? 0);

  const expectedVersion = linkedSignal<IdVersion, IdVersion>({
    source: () => ({ id: id(), version: dataVersion() }),
    computation: (source, previous) => {
      const prevId = previous?.value.id;
      const prevVersion = previous?.value.version ?? 0;

      if (prevVersion < source.version || prevId !== source.id) {
        return source;
      }

      // keep the expected version if it's higher than the data's version
      return { ...source, version: prevVersion };
    },
  });

  const updateExpected = computed(() => expectedVersion().version > dataVersion());

  // Only update the expected version if the data has still the same id.
  // A command can be in flight, while the component loads data for another id.
  const setExpectedVersion = (id: string, version: number) =>
    expectedVersion.update((v) => (v.id === id && version > v.version ? { ...v, version } : v));

  return {
    updateExpected,
    setExpectedVersion,
  };
};
