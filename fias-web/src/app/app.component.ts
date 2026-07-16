import { Component } from '@angular/core';
import { FiasSearchComponent } from './search/fias-search.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FiasSearchComponent],
  template: `
    <header class="app-header">
      <h1>Поиск по ФИАС / ГАР</h1>
      <p>Полнотекстовый поиск и поиск по полям адресных объектов</p>
    </header>
    <main class="app-main">
      <app-fias-search></app-fias-search>
    </main>
  `,
  styles: [
    `
      .app-header {
        background: #0d47a1;
        color: #fff;
        padding: 22px 24px;
      }
      .app-header h1 {
        margin: 0;
        font-size: 22px;
      }
      .app-header p {
        margin: 6px 0 0;
        opacity: 0.85;
        font-size: 14px;
      }
      .app-main {
        max-width: 980px;
        margin: 0 auto;
        padding: 24px 16px 48px;
      }
    `,
  ],
})
export class AppComponent {}
