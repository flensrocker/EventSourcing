import { Component, computed, input } from '@angular/core';

import { MatListModule } from '@angular/material/list';

import { TaskQueryResponse_List_ListItem } from '@flensrocker/to-do-core';

@Component({
  selector: 'to-do-task-list-item',
  imports: [MatListModule],
  template: `<span matListItemTitle>{{ title() }}</span>`,
})
export class ToDoTaskListItem {
  readonly item = input.required<TaskQueryResponse_List_ListItem>();

  protected readonly title = computed(() => this.item().Title);
}
