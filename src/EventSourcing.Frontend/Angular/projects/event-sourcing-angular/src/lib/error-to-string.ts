import { HttpErrorResponse } from '@angular/common/http';
import { Pipe, PipeTransform } from '@angular/core';

export const errorToString = (error: unknown): string => {
  if (error == null || error === 0n || error === 0 || error === '') {
    return '';
  }

  if (error instanceof HttpErrorResponse) {
    if (typeof error.error === 'string') {
      return error.error;
    } else {
      return `Error ${error.status}`;
    }
  } else if (error instanceof Error) {
    return error.message;
  }

  return 'unexpected error';
};

@Pipe({
  name: 'frEsError',
  pure: true,
})
export class FrEsErrorPipe implements PipeTransform {
  transform(value: unknown, ..._args: unknown[]) {
    return errorToString(value);
  }
}
