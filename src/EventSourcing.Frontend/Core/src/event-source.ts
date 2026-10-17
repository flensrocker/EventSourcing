import { NEVER, Observable, defer, filter, finalize, fromEvent, map, merge, of, scan, shareReplay, startWith, switchMap } from 'rxjs';

type EventSourceMessageParseFn<TData> = (eventType: string, message: string) => TData;

export type EventSourceOptions<TData> = Readonly<{
  eventTypes?: readonly string[];
  parseFn?: EventSourceMessageParseFn<TData>;
  allowedOrigins?: readonly string[];
}>;

const defaultEventTypes = ['message'];
const defaultParseFn: EventSourceMessageParseFn<string> = (_, message) => message;

export type EventSourceStatus<TData> = Readonly<{
  status: typeof EventSource.CONNECTING;
  lastValue: TData | undefined;
}>;
export type EventSourceValue<TData> = Readonly<{
  value: TData;
}>;
export type EventSourceData<TData> = EventSourceStatus<TData> | EventSourceValue<TData>;

const defaultStatus: EventSourceStatus<undefined> = { status: EventSource.CONNECTING, lastValue: undefined };

export const isEventSourceStatus = <TData>(data: EventSourceData<TData>): data is EventSourceStatus<TData> => 'status' in data;
export const isEventSourceValue = <TData>(data: EventSourceData<TData>): data is EventSourceValue<TData> => 'value' in data;

export const fromEventSource = <TData = string>(
  sse: EventSource,
  options?: EventSourceOptions<TData>,
): Observable<EventSourceData<TData>> => {
  const eventTypes = options?.eventTypes == null || options.eventTypes.length === 0 ? defaultEventTypes : options.eventTypes;
  const parseFn = options?.parseFn ?? (defaultParseFn as EventSourceMessageParseFn<TData>);
  const allowedOrigins = options?.allowedOrigins;

  const events$ = eventTypes.map((eventType) =>
    fromEvent<MessageEvent<string>>(sse, eventType).pipe(
      filter((event) => allowedOrigins == null || allowedOrigins.includes(event.origin)),
      map((event): EventSourceData<TData> => ({
        value: parseFn(eventType, event.data),
      })),
    ),
  );

  const error$: Observable<EventSourceStatus<TData>> = fromEvent<Event>(sse, 'error').pipe(
    switchMap((): Observable<EventSourceStatus<TData>> => {
      if (sse.readyState === EventSource.CLOSED) {
        throw new Error(`An error occurred while attempting to connect${sse?.url ? `: ${sse.url}` : '.'}`);
      }

      if (sse.readyState === EventSource.OPEN) {
        return NEVER;
      }

      return of(defaultStatus);
    }),
    startWith(defaultStatus),
  );

  return merge(...events$, error$).pipe(
    scan<EventSourceData<TData>, [EventSourceData<TData>, EventSourceData<TData>]>(
      (last, data) => {
        if (isEventSourceStatus(data)) {
          if (isEventSourceValue(last[1])) {
            data = { ...data, lastValue: last[1].value };
          } else {
            data = { ...data, lastValue: last[1].lastValue };
          }
        }

        return [data, data];
      },
      [defaultStatus, defaultStatus],
    ),
    map(([_, data]) => data),
    shareReplay({ refCount: true }),
  );
};

export const connectEventSource = <TData = string>(
  url: string,
  configuration?: EventSourceInit,
  options?: EventSourceOptions<TData>,
): Observable<EventSourceData<TData>> => {
  return defer(() => {
    const sse = new EventSource(url, configuration);

    return fromEventSource<TData>(sse, options).pipe(
      finalize(() => {
        sse.close();
      }),
    );
  });
};
