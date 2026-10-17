import { busyTracker } from './busy-tracker';

describe('busyTracker', () => {
  it('should create a non-busy tracker', () => {
    const sut = busyTracker();

    expect(sut).toBeDefined();
    expect(sut.isBusy()).toBeFalsy();
  });

  it('should be busy after start', () => {
    const sut = busyTracker();

    sut.start();

    expect(sut.isBusy()).toBeTruthy();
  });

  it('should be busy after multiple starts and fewer stops', () => {
    const sut = busyTracker();

    const stop1 = sut.start();
    sut.start();
    stop1();

    expect(sut.isBusy()).toBeTruthy();
  });

  it('should not be busy after stop', () => {
    const sut = busyTracker();

    const stop = sut.start();
    stop();

    expect(sut.isBusy()).toBeFalsy();
  });
});
