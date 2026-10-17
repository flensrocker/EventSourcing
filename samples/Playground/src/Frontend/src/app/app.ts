import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ToDoAngular } from '@flensrocker/to-do-angular';

@Component({
  selector: 'pg-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, ToDoAngular],
  template: `<to-do-angular /><router-outlet />`,
})
export class App {}
