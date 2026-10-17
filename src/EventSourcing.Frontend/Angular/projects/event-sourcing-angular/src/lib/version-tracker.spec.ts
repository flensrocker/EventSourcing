import { signal } from '@angular/core';

import { versionTracker } from './version-tracker';

describe('versionTracker', () => {
  it('should create a version-tracker', () => {
    const id = signal('id-1');
    const data = signal({ Version: 1, Data: 'data' });

    const sut = versionTracker(id, data);

    expect(sut).toBeDefined();
  });
});
