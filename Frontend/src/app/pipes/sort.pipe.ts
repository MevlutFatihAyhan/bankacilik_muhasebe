import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'sort',
  standalone: true
})
export class SortPipe implements PipeTransform {
  transform(items: any[], column: string, direction: 'asc' | 'desc' = 'asc'): any[] {
    if (!items || !column) return items;

    return [...items].sort((a, b) => {
      let valA = a[column];
      let valB = b[column];

      // Convert to strings for comparison if they are numbers
      if (typeof valA === 'string' && typeof valB === 'string') {
        valA = valA.toLowerCase();
        valB = valB.toLowerCase();
      }

      if (valA < valB) {
        return direction === 'asc' ? -1 : 1;
      }
      if (valA > valB) {
        return direction === 'asc' ? 1 : -1;
      }
      return 0;
    });
  }
}
