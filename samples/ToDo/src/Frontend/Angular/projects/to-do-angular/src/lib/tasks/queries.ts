import { HttpParams, httpResource, HttpResourceRequest } from '@angular/common/http';
import { Injector, Signal } from '@angular/core';

import {
  getTaskQueryDailyStatsRoute,
  getTaskQueryListRoute,
  TaskQueryRequest_DailyStats,
  TaskQueryRequest_List,
  TaskQueryResponse_DailyStats,
  TaskQueryResponse_List,
} from '@flensrocker/to-do-core';

export const toDoTaskQueryDailyStats = (query: Signal<TaskQueryRequest_DailyStats | null | undefined>, injector?: Injector) =>
  httpResource<TaskQueryResponse_DailyStats>(
    (): HttpResourceRequest | undefined => {
      const params = query();
      if (params == null) {
        return undefined;
      }

      let httpParams = new HttpParams();
      if (params.TenantId != null) {
        httpParams = httpParams.append('TenantId', params.TenantId);
      }
      if (params.UserId != null) {
        httpParams = httpParams.append('UserId', params.UserId);
      }
      if (params.FromDate != null) {
        httpParams = httpParams.append('FromDate', params.FromDate);
      }
      if (params.ToDate != null) {
        httpParams = httpParams.append('ToDate', params.ToDate);
      }

      return {
        url: getTaskQueryDailyStatsRoute(),
        params: httpParams,
      };
    },
    // TODO use parse with validation schema
    { injector },
  );

export const toDoTaskQueryList = (query: Signal<TaskQueryRequest_List | null | undefined>, injector?: Injector) =>
  httpResource<TaskQueryResponse_List>(
    (): HttpResourceRequest | undefined => {
      const params = query();
      if (params == null) {
        return undefined;
      }

      let httpParams = new HttpParams();
      if (params.TenantId != null) {
        httpParams = httpParams.append('TenantId', params.TenantId);
      }

      return {
        url: getTaskQueryListRoute(),
        params: httpParams,
      };
    },
    // TODO use parse with validation schema
    { injector },
  );
