import { computed, linkedSignal, Signal } from '@angular/core';

import { IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

type IdVersion = Readonly<{
  id: string;
  version: number;
}>;

export type VersionTracker = Readonly<{
  /**
   * Switches to `true` if the provided id, version and actual data differs and new data is expected.
   */
  updateExpected: Signal<boolean>;
  /**
   * Use to set the expected version the data should have.
   * Updates the version only if the provided id matches the expected id and the expected version is higher than the current one.
   * @param id The id that the data had before the new version arrived.
   * @param version The version that the data should have eventually.
   */
  setExpectedVersion: (id: string, version: number) => void;
}>;

/**
 * Tracks the "loading id" and the "loaded data" if an update of the data should be expected.
 * The usual use case:
 * - A component receives an id and uses that to trigger the loading of the data via Server Sent Events (SSE).
 * - The loaded data also contains the id and a version.
 * - The component uses commands to mutate the data. The command returns the new version the data should have.
 * - The version-tracker gets updated with the expected version and switches its `updateExpected` to `true`.
 * - The server sends the updated data via SSE to the component.
 * - The id and expected version match again and the `updateExpected` signal resets to `false`.
 * With this a spinner can be shown starting when sending the command and stopping when the updated data arrives.
 * @param id The signal with the id of the data that will be loaded
 * @param data The signal that contains the loaded data
 * @param getId A function that extracts the id of the loaded data
 * @returns
 */
export const versionTracker = <T extends IDomainEventVersionable>(
  id: Signal<string>,
  data: Signal<T | null | undefined>,
  getId: (data: T) => string,
): VersionTracker => {
  const dataId = computed(() => {
    const d = data();
    return d == null ? undefined : getId(d);
  });
  const dataVersion = computed(() => data()?.Version ?? 0);

  const expectedVersion = linkedSignal<IdVersion, IdVersion>({
    source: () => ({ id: id(), version: dataVersion() }),
    computation: (source, previous) => {
      const prevId = previous?.value.id;
      const prevVersion = previous?.value.version ?? 0;

      if (prevId !== source.id || prevVersion < source.version) {
        return source;
      }

      // keep the expected version if it's higher than the data's version
      return { ...source, version: prevVersion };
    },
  });

  const updateExpected = computed(() => expectedVersion().id !== dataId() || expectedVersion().version > dataVersion());

  // Only update the expected version if the data has still the same id.
  // A command can be in flight, while the component loads data for another id.
  const setExpectedVersion = (id: string, version: number) =>
    expectedVersion.update((v) => (v.id === id && version > v.version ? { ...v, version } : v));

  return {
    updateExpected,
    setExpectedVersion,
  };
};
