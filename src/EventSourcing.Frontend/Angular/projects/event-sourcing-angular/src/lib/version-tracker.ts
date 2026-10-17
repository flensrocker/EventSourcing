import { computed, linkedSignal, Signal } from '@angular/core';

import { IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

type IdVersion = Readonly<{
  id: string;
  version: number;
}>;

export const versionTracker = <T extends IDomainEventVersionable>(id: Signal<string>, data: Signal<T | null | undefined>) => {
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

  const updateExpected = computed(() => expectedVersion().version > actualVersion());

  const setExpectedVersion = (id: string, version: number) => expectedVersion.update((v) => (v.id === id ? { ...v, version } : v));

  return {
    updateExpected,
    setExpectedVersion,
  };
};
