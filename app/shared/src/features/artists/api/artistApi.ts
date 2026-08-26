import { apiClient } from "../../../lib/apiClient";
import type {
  Artist,
  CreateArtistRequest,
  UpdateArtistRequest,
} from "../types";
import type { ImageFile } from "../../../types/image";
import type { Genre } from "../../../types/common";

export interface CreateArtist {
  name: string;
  about: string;
  latitude: number;
  longitude: number;
  genres: Genre[];
  banner: File;
  avatar: File;
}

type FormDataValue = Parameters<FormData["append"]>[1];

function toCreateFormData(
  request: CreateArtistRequest | CreateArtist,
): FormData {
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

async function createArtist(
  request: CreateArtistRequest | CreateArtist,
): Promise<Artist> {
  const { data } = await apiClient.post<Artist>(
    "/organization/artist",
    toCreateFormData(request),
  );
  return data;
}

async function updateArtist(request: UpdateArtistRequest): Promise<Artist>;
async function updateArtist(
  artist: Artist,
  banner?: ImageFile,
  avatar?: ImageFile,
): Promise<Artist>;
async function updateArtist(
  requestOrArtist: UpdateArtistRequest | Artist,
  banner?: ImageFile,
  avatar?: ImageFile,
): Promise<Artist> {
  const request =
    "id" in requestOrArtist
      ? {
          name: requestOrArtist.name,
          about: requestOrArtist.about,
          latitude: requestOrArtist.latitude,
          longitude: requestOrArtist.longitude,
          genres: requestOrArtist.genres,
          banner,
          avatar,
        }
      : requestOrArtist;
  const { data } = await apiClient.put<Artist>(
    "/organization/artist",
    toUpdateFormData(request),
  );
  return data;
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
    return data ?? undefined;
  },

  createArtist,
  updateArtist,
};

export default artistApi;
