import { busyTracker } from './busy-tracker';

describe('busyTracker', () => {
  it('should create in a non-busy state', () => {
    const sut = busyTracker();

    expect(sut).toBeDefined();
    expect(sut.isBusy()).toBeFalsy();
  });

  it('should be busy after start', () => {
    const sut = busyTracker();

    expect(sut.isBusy()).toBeFalsy();

    sut.start();

    expect(sut.isBusy()).toBeTruthy();
  });

  it('should not be busy after stop', () => {
    const sut = busyTracker();

    const stop = sut.start();
    stop();

    expect(sut.isBusy()).toBeFalsy();
  });

  it('should still be busy after multiple starts and fewer stops', () => {
    const sut = busyTracker();

    expect(sut.isBusy()).toBeFalsy();

    const stop1 = sut.start();

    expect(sut.isBusy()).toBeTruthy();

    const stop2 = sut.start();

    expect(sut.isBusy()).toBeTruthy();

    stop1();

    expect(sut.isBusy()).toBeTruthy();

    stop2();

    expect(sut.isBusy()).toBeFalsy();
  });
});
