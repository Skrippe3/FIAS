import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { FiasSearchRequest, FiasSearchResponse } from './models';

@Injectable({ providedIn: 'root' })
export class FiasSearchService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/fias/search`;

  constructor(private readonly http: HttpClient) {}

  search(request: FiasSearchRequest): Observable<FiasSearchResponse> {
    let params = new HttpParams()
      .set('onlyActive', request.onlyActive)
      .set('page', request.page)
      .set('pageSize', request.pageSize);

    if (request.query) {
      params = params.set('query', request.query);
    }
    if (request.typeName) {
      params = params.set('typeName', request.typeName);
    }
    if (request.levelId != null) {
      params = params.set('levelId', request.levelId);
    }
    if (request.regionCode) {
      params = params.set('regionCode', request.regionCode);
    }

    return this.http.get<FiasSearchResponse>(this.baseUrl, { params });
  }
}
