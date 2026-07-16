import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FiasSearchService } from './fias-search.service';
import {
  FiasAddressSearchResult,
  FiasSearchRequest,
  FiasSearchResponse,
} from './models';

@Component({
  selector: 'app-fias-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fias-search.component.html',
  styleUrls: ['./fias-search.component.css'],
})
export class FiasSearchComponent {
  request: FiasSearchRequest = {
    query: '',
    typeName: '',
    levelId: null,
    regionCode: '',
    onlyActive: true,
    page: 1,
    pageSize: 20,
  };

  items: FiasAddressSearchResult[] = [];
  total = 0;
  loading = false;
  error: string | null = null;
  searched = false;

  constructor(private readonly searchService: FiasSearchService) {}

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.request.pageSize));
  }

  submit(): void {
    this.request.page = 1;
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.request.page) {
      return;
    }
    this.request.page = page;
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.error = null;

    this.searchService.search(this.request).subscribe({
      next: (response: FiasSearchResponse) => {
        this.items = response.items;
        this.total = response.total;
        this.searched = true;
        this.loading = false;
      },
      error: (err) => {
        this.error =
          'Не удалось выполнить поиск. Проверьте, запущен ли Fias.Api.';
        console.error(err);
        this.loading = false;
      },
    });
  }
}
