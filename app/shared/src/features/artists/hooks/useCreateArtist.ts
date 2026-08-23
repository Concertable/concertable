import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import type { ImageFile } from "../../../types/image";
import artistApi from "../api/artistApi";
import { createArtistRequestSchema } from "../schemas/artistRequestSchemas";
import type { Artist, CreateArtistRequest } from "../types";
import { artistKeys } from "./useArtistQuery";

export interface UseCreateArtistOptions {
  onSuccess?: (saved: Artist) => void;
}

export interface UseCreateArtistResult {
  draft: Artist;
  isCreating: boolean;
  canCreate: boolean;
  createError?: string;
  create: () => void;
  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setBanner: (banner: ImageFile) => void;
  setAvatar: (avatar: ImageFile) => void;
}

const defaultValues = {
  name: "",
  about: "",
  latitude: 51.5074,
  longitude: -0.1278,
  genres: [],
} satisfies Partial<CreateArtistRequest>;

export function useCreateArtist(
  options?: UseCreateArtistOptions,
): UseCreateArtistResult {
  const queryClient = useQueryClient();
  const {
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isValid },
  } = useForm<CreateArtistRequest>({
    resolver: zodResolver(createArtistRequestSchema),
    defaultValues,
    mode: "onChange",
  });

  const mutation = useMutation({
    mutationFn: artistApi.createArtist,
    onSuccess: (saved) => {
      queryClient.setQueryData(artistKeys.my(), saved);
      queryClient.setQueryData(artistKeys.byId(saved.id), saved);
      options?.onSuccess?.(saved);
    },
  });

  const request = watch();
  const draft: Artist = {
    id: 0,
    name: request.name,
    about: request.about,
    bannerUrl: request.banner?.uri ?? "",
    avatar: request.avatar?.uri,
    rating: 0,
    genres: request.genres,
    email: "",
    county: "",
    town: "",
    latitude: request.latitude,
    longitude: request.longitude,
  };

  const create = () => {
    void handleSubmit((request) => mutation.mutate(request))();
  };

  const createError =
    errors.name?.message ??
    errors.about?.message ??
    errors.latitude?.message ??
    errors.longitude?.message ??
    errors.genres?.message ??
    errors.banner?.message ??
    errors.avatar?.message;

  return {
    draft,
    isCreating: mutation.isPending,
    canCreate: isValid,
    createError,
    create,
    setName: (name) =>
      setValue("name", name, { shouldDirty: true, shouldValidate: true }),
    setAbout: (about) =>
      setValue("about", about, { shouldDirty: true, shouldValidate: true }),
    setBanner: (banner) =>
      setValue("banner", banner, { shouldDirty: true, shouldValidate: true }),
    setAvatar: (avatar) =>
      setValue("avatar", avatar, { shouldDirty: true, shouldValidate: true }),
  };
}
