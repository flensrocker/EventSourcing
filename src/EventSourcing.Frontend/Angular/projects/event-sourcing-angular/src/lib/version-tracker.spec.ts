import { signal, WritableSignal } from '@angular/core';

import { IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

import { versionTracker } from './version-tracker';

type Data = IDomainEventVersionable &
  Readonly<{
    DataId: string;
    Data: string;
  }>;

const getDataId = (data: Data) => data.DataId;

describe('versionTracker', () => {
  let id: WritableSignal<string>;
  let data: WritableSignal<Data>;

  beforeEach(() => {
    id = signal('06630de3-9b62-49d9-bbe9-b218044517b6');
    data = signal({ DataId: id(), Version: 2, Data: 'data' });
  });

  it('should create in a state not expecting an update', () => {
    const sut = versionTracker(id, data, getDataId);

    expect(sut).toBeDefined();
    const updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();
  });

  it('should expect an update if the expected version is higher and reset if data catches up', () => {
    const sut = versionTracker(id, data, getDataId);

    let updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();

    sut.setExpectedVersion(id(), data().Version + 1);

    updateExpected = sut.updateExpected();
    expect(updateExpected).toBeTruthy();

    data.update((d) => ({ ...d, Version: d.Version + 1 }));

    updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();
  });

  it('should expect an update if the expected id is different', () => {
    const sut = versionTracker(id, data, getDataId);

    let updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();

    const newId = '7ab46f74-d616-4efa-a0be-fee268a93feb';
    id.set(newId);

    updateExpected = sut.updateExpected();
    expect(updateExpected).toBeTruthy();

    data.update((d) => ({ ...d, DataId: newId }));

    updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();
  });

  it('should not expect an update if the expected version is lower', () => {
    const sut = versionTracker(id, data, getDataId);

    let updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();

    sut.setExpectedVersion(id(), data().Version - 1);

    updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();
  });

  it('should not set the expected version if the id is different', () => {
    const sut = versionTracker(id, data, getDataId);

    let updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();

    sut.setExpectedVersion('3fef241d-3c8d-49dd-8e1b-b840d483c728', data().Version + 1);

    updateExpected = sut.updateExpected();
    expect(updateExpected).toBeFalsy();
  });
});
