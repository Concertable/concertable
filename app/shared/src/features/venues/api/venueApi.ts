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

const venueApi = {
  getVenue: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`/venue/${id}`);
    return data;
  },

  getVenueById: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`/venue/${id}`);
    return data;
  },

  getMyVenue: async (): Promise<Venue | undefined> => {
    const { data } = await apiClient.getOptional<Venue>("/organization/venue");
    return data;
  },

  createVenue: async (request: CreateVenueRequest): Promise<Venue> => {
    const { data } = await apiClient.post<Venue>(
      "/organization/venue",
      toCreateFormData(request),
    );
    return data;
  },

  updateVenue: async (request: UpdateVenueRequest): Promise<Venue> => {
    const { data } = await apiClient.put<Venue>(
      "/organization/venue",
      toUpdateFormData(request),
    );
    return data;
  },
};

export default venueApi;
