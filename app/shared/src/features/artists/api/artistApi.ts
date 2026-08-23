import { apiClient } from "../../../lib/apiClient";
import type {
  Artist,
  CreateArtistRequest,
  UpdateArtistRequest,
} from "../types";

type FormDataValue = Parameters<FormData["append"]>[1];

function toCreateFormData(request: CreateArtistRequest): FormData {
  const formData = new FormData();
  formData.append("Name", request.name);
  formData.append("About", request.about);
  formData.append("Latitude", String(request.latitude));
  formData.append("Longitude", String(request.longitude));
  request.genres.forEach((genre, index) => {
    formData.append(`Genres[${index}]`, genre);
  });
  formData.append("Banner", request.banner as unknown as FormDataValue);
  formData.append("Avatar", request.avatar as unknown as FormDataValue);
  return formData;
}

function toUpdateFormData(request: UpdateArtistRequest): FormData {
  const formData = new FormData();
  formData.append("Name", request.name);
  formData.append("About", request.about);
  formData.append("Latitude", String(request.latitude));
  formData.append("Longitude", String(request.longitude));
  request.genres.forEach((genre, index) => {
    formData.append(`Genres[${index}]`, genre);
  });
  if (request.banner) {
    formData.append("Banner", request.banner as unknown as FormDataValue);
  }
  if (request.avatar) {
    formData.append("Avatar", request.avatar as unknown as FormDataValue);
  }
  return formData;
}

const artistApi = {
  getArtist: async (id: number): Promise<Artist> => {
    const { data } = await apiClient.get<Artist>(`/artist/${id}`);
    return data;
  },

  getArtistById: async (id: number): Promise<Artist> => {
    const { data } = await apiClient.get<Artist>(`/artist/${id}`);
    return data;
  },

  getMyArtist: async (): Promise<Artist | undefined> => {
    const { data } = await apiClient.getOptional<Artist>("/organization/artist");
    return data;
  },

  createArtist: async (request: CreateArtistRequest): Promise<Artist> => {
    const { data } = await apiClient.post<Artist>(
      "/organization/artist",
      toCreateFormData(request),
    );
    return data;
  },

  updateArtist: async (request: UpdateArtistRequest): Promise<Artist> => {
    const { data } = await apiClient.put<Artist>(
      "/organization/artist",
      toUpdateFormData(request),
    );
    return data;
  },
};

export default artistApi;
