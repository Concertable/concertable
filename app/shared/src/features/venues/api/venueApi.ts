import { apiClient } from "../../../lib/apiClient";
import type {
  CreateVenueRequest,
  UpdateVenueRequest,
  Venue,
} from "../types";
import type { ImageFile } from "../../../types/image";

export interface CreateVenue {
  name: string;
  about: string;
  latitude: number;
  longitude: number;
  banner: File;
  avatar: File;
}

type FormDataValue = Parameters<FormData["append"]>[1];

function toCreateFormData(
  request: CreateVenueRequest | CreateVenue,
): FormData {
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

async function createVenue(
  request: CreateVenueRequest | CreateVenue,
): Promise<Venue> {
  const { data } = await apiClient.post<Venue>(
    "/organization/venue",
    toCreateFormData(request),
  );
  return data;
}

async function updateVenue(request: UpdateVenueRequest): Promise<Venue>;
async function updateVenue(
  venue: Venue,
  banner?: ImageFile,
  avatar?: ImageFile,
): Promise<Venue>;
async function updateVenue(
  requestOrVenue: UpdateVenueRequest | Venue,
  banner?: ImageFile,
  avatar?: ImageFile,
): Promise<Venue> {
  const request =
    "id" in requestOrVenue
      ? {
          name: requestOrVenue.name,
          about: requestOrVenue.about,
          latitude: requestOrVenue.latitude,
          longitude: requestOrVenue.longitude,
          banner,
          avatar,
        }
      : requestOrVenue;
  const { data } = await apiClient.put<Venue>(
    "/organization/venue",
    toUpdateFormData(request),
  );
  return data;
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
    return data ?? undefined;
  },

  createVenue,
  updateVenue,
};

export default venueApi;
