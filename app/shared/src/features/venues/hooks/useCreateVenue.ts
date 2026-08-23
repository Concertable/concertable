import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import type { ImageFile } from "../../../types/image";
import venueApi from "../api/venueApi";
import { createVenueRequestSchema } from "../schemas/venueRequestSchemas";
import type { CreateVenueRequest, Venue } from "../types";
import { venueKeys } from "./useVenueQuery";

export interface UseCreateVenueOptions {
  onSuccess?: (saved: Venue) => void;
}

export interface UseCreateVenueResult {
  draft: Venue;
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
} satisfies Partial<CreateVenueRequest>;

export function useCreateVenue(
  options?: UseCreateVenueOptions,
): UseCreateVenueResult {
  const queryClient = useQueryClient();
  const {
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isValid },
  } = useForm<CreateVenueRequest>({
    resolver: zodResolver(createVenueRequestSchema),
    defaultValues,
    mode: "onChange",
  });

  const mutation = useMutation({
    mutationFn: venueApi.createVenue,
    onSuccess: (saved) => {
      queryClient.setQueryData(venueKeys.my(), saved);
      queryClient.setQueryData(venueKeys.byId(saved.id), saved);
      options?.onSuccess?.(saved);
    },
  });

  const request = watch();
  const draft: Venue = {
    id: 0,
    name: request.name,
    about: request.about,
    bannerUrl: request.banner?.uri ?? "",
    avatar: request.avatar?.uri,
    rating: 0,
    county: "",
    town: "",
    email: "",
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
