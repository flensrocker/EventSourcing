export type IDomainEventVersionable = Readonly<{
  Version: number;
}>;

export const isDomainEventVersionable = (obj: unknown): obj is IDomainEventVersionable =>
  obj != null && typeof obj === 'object' && 'Version' in obj && typeof obj.Version === 'number';
