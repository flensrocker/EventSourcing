export const $type = '$type';
export type $type = typeof $type;

export type WithType<Type> = {
  readonly [$type]: Type;
};

export type WithoutType<T extends WithType<Type>, Type = T[$type]> = Omit<T, $type>;

export const isObjectOfType = <T extends WithType<Type>, Type = T[$type]>(obj: unknown, type: Type): obj is T =>
  obj != null && typeof obj === 'object' && $type in obj && obj.$type === type;

export const withType = <T extends WithType<Type>, Type = T[$type]>(obj: WithoutType<T, Type>, type: Type): T =>
  ({ [$type]: type, ...obj }) as T;
