import { type IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

import { getToDoTaskRoute, TaskStatus } from './constants';

// TODO create validation schema for all types
export type TaskQueryRequest_DailyStats = Readonly<{
  TenantId: string | null;
  UserId: string | null;
  FromDate: string | null;
  ToDate: string | null;
}>;
export type TaskQueryResponse_DailyStats_DailyStatsItem = Readonly<{
  Day: string;
  UserId: string | null;
  CountNoted: number;
  CountCompleted: number;
  CountDismissed: number;
}>;
export type TaskQueryResponse_DailyStats = Readonly<{
  ItemList: readonly TaskQueryResponse_DailyStats_DailyStatsItem[];
}>;
export const getTaskQueryDailyStatsRoute = () => getToDoTaskRoute('dailystats');

export type TaskQueryRequest_List = Readonly<{
  TenantId: string | null;
}>;
export type TaskQueryResponse_List_ListItem = IDomainEventVersionable &
  Readonly<{
    TaskId: string;
    CreatedAt: string;
    Title: string;
    Status: TaskStatus;
  }>;
export type TaskQueryResponse_List = Readonly<{
  ItemList: readonly TaskQueryResponse_List_ListItem[];
}>;
export const getTaskQueryListRoute = () => getToDoTaskRoute();
