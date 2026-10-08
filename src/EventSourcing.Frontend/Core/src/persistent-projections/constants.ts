import * as EventSourcingConstants from '../constants';

export const Base = `${EventSourcingConstants.AliasBase}_PersistentProjectionEvent`;

export const ProjectionRegistered = `${Base}_ProjectionRegistered`;
export const ProjectionRebuilt = `${Base}_ProjectionRebuilt`;
