import { apiClient } from "../../../lib/apiClient";
import type {
  CreateVenueRequest,
  UpdateVenueRequest,
  Venue,
} from "../types";

type FormDataValue = Parameters<FormData["append"]>[1];

function toCreateFormData(request: CreateVenueRequest): FormData {
  const formData = new FormData();
  formData.append("Name", request.name);
  formData.append("About", request.about);
  formData.append("Latitude", String(request.latitude));
  formData.append("Longitude", String(request.longitude));
  formData.append("Banner", request.banner as unknown as FormDataValue);
  formData.append("Avatar", request.avatar as unknown as FormDataValue);
  return formData;
}

function toUpdateFormData(request: UpdateVenueRequest): FormData {
  const formData = new FormData();
  formData.append("Name", request.name);
  formData.append("About", request.about);
  formData.append("Latitude", String(request.latitude));
  formData.append("Longitude", String(request.longitude));
  if (request.banner) {
    formData.append("Banner", request.banner as unknown as FormDataValue);
  }
  if (request.avatar) {
    formData.append("Avatar", request.avatar as unknown as FormDataValue);
  }
  return formData;
}

const BASE = "/venue";
const ORGANIZATION_BASE = "/organization/venue";

const venueApi = {
  getVenue: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`${BASE}/${id}`);
    return data;
  },

  getVenueById: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`${BASE}/${id}`);
    return data;
  },

  getMyVenue: async (): Promise<Venue | null> => {
    const { data } = await apiClient.getOptional<Venue>(ORGANIZATION_BASE);
    return data;
  },

  createVenue: async (request: CreateVenueRequest): Promise<Venue> => {
    const { data } = await apiClient.post<Venue>(
      ORGANIZATION_BASE,
      toCreateFormData(request),
    );
    return data;
  },

  updateVenue: async (request: UpdateVenueRequest): Promise<Venue> => {
    const { data } = await apiClient.put<Venue>(
      ORGANIZATION_BASE,
      toUpdateFormData(request),
    );
    return data;
  },
};

export default venueApi;
