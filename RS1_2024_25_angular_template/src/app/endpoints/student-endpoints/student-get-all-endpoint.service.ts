import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {MyPagedRequest} from '../../helper/my-paged-request';
import {MyConfig} from '../../my-config';
import {buildHttpParams} from '../../helper/http-params.helper';
import {MyBaseEndpointAsync} from '../../helper/my-base-endpoint-async.interface';
import {MyPagedList} from '../../helper/my-paged-list';

// DTO za zahtjev
export interface StudentGetAllRequest extends MyPagedRequest {
  q?: string; // Upit za pretragu (ime, prezime, broj indeksa, itd.)
  isDeleted?: boolean;
}

// DTO za odgovor
export interface StudentGetAllResponse {
  id: number;
  firstName: string;
  lastName: string;
  studentNumber: string;
  citizenship?: string; // Državljanstvo
  birthMunicipality?: string; // Općina rođenja
  Obrisan: boolean;
}

export interface postStudent{
  firstName: string;
  lastName: string;
  studentNumber: string;
  citizenshipId?: number // Državljanstvo
  birthMunicipalityId?: number;
}

export interface winterDTO{
  studentId: number;
  datumZimskiUpis: string,
  akademskaGodinaId: number,
  godinaStudija: number,
  cijenaSkolarine: number,
  obnova: boolean,
  evidentirao: string
}

@Injectable({
  providedIn: 'root',
})
export class StudentGetAllEndpointService
  implements MyBaseEndpointAsync<StudentGetAllRequest, MyPagedList<StudentGetAllResponse>> {
  private apiUrl = `${MyConfig.api_address}/students`;

  constructor(private httpClient: HttpClient) {
  }

  handleAsync(request: StudentGetAllRequest) {
    const params = buildHttpParams(request); // Pretvori DTO u query parametre
    return this.httpClient.get<MyPagedList<StudentGetAllResponse>>(`${this.apiUrl}/filter`, {params});
  }

  getContries() {
    return this.httpClient.get<any>(`${this.apiUrl}/getContries`);
  }

  getRegions(){
    return this.httpClient.get<any>(`${this.apiUrl}/getRegions`);
  }

  postStudent(student: postStudent){
    return this.httpClient.post<any>(`${this.apiUrl}/postStudent`, student);
  }

  deleteStudent(id: number) {
    return this.httpClient.put<any>(`${this.apiUrl}/deleteStudent`, id);
  }

  obnoviStudent(id: number) {
    return this.httpClient.put<any>(`${this.apiUrl}/obnoviStudent`, id);
  }

  getStudent(id: number) {
    return this.httpClient.get<any>(`${this.apiUrl}/getStudent/${id}`);
  }

  getStudijaGodina(id: number) {
    return this.httpClient.get<any>(`${this.apiUrl}/getStudijaGodina/${id}`);
  }

  getAkademskeGodine(){
    return this.httpClient.get<any>(`${this.apiUrl}/getAkademskeGodine`);
  }
}
