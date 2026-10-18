import { signal, WritableSignal } from '@angular/core';

import { IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

import { versionTracker } from './version-tracker';

type Data = IDomainEventVersionable &
  Readonly<{
    Data: string;
  }>;

describe('versionTracker', () => {
  let id: WritableSignal<string>;
  let data: WritableSignal<Data>;

  beforeEach(() => {
    id = signal('id-1');
    data = signal({ Version: 2, Data: 'data' });
  });

  it('should create in a state not expecting an update', () => {
    const sut = versionTracker(id, data);

    expect(sut).toBeDefined();
    expect(sut.updateExpected()).toBeFalsy();
  });

  it('should expect an update if the expected version is higher and reset if data catches up', () => {
    const sut = versionTracker(id, data);

    expect(sut.updateExpected()).toBeFalsy();

    sut.setExpectedVersion(id(), data().Version + 1);

    expect(sut.updateExpected()).toBeTruthy();

    data.update((d) => ({ ...d, Version: d.Version + 1 }));

    expect(sut.updateExpected()).toBeFalsy();
  });

  it('should not expect an update if the expected version is lower', () => {
    const sut = versionTracker(id, data);

    expect(sut.updateExpected()).toBeFalsy();

    sut.setExpectedVersion(id(), data().Version - 1);

    expect(sut.updateExpected()).toBeFalsy();
  });

  it('should not set the expected version if the id is different', () => {
    const sut = versionTracker(id, data);

    expect(sut.updateExpected()).toBeFalsy();

    sut.setExpectedVersion(id() + '-2', data().Version + 1);

    expect(sut.updateExpected()).toBeFalsy();
  });
});
