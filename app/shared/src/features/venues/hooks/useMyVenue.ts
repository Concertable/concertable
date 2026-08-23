import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import type { ImageFile } from "../../../types/image";
import venueApi from "../api/venueApi";
import { updateVenueRequestSchema } from "../schemas/venueRequestSchemas";
import { Venue, type UpdateVenueRequest } from "../types";
import { venueKeys, useMyVenueQuery } from "./useVenueQuery";
import type { UseVenueResult } from "./useVenue";

export interface UseMyVenueOptions {
  onSuccess?: (saved: Venue) => void;
  afterSave?: () => Promise<void>;
  onToggleEdit?: () => void;
  onResetDraft?: () => void;
  extraDirty?: boolean;
}

export interface UseMyVenueResult extends UseVenueResult {
  draft: Venue | undefined;
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  canSave: boolean;
  saveError?: string;
  save: () => void;
  toggleEdit: () => void;
  resetDraft: () => void;
  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setBanner: (banner: ImageFile) => void;
  setAvatar: (avatar: ImageFile) => void;
  setLocation: (
    latitude: number,
    longitude: number,
    county: string,
    town: string,
  ) => void;
}

const emptyRequest: UpdateVenueRequest = {
  name: "",
  about: "",
  latitude: 0,
  longitude: 0,
};

export function useMyVenue(options?: UseMyVenueOptions): UseMyVenueResult {
  const query = useMyVenueQuery();
  const queryClient = useQueryClient();
  const [editMode, setEditMode] = useState(false);
  const [location, setLocationDisplay] = useState({ county: "", town: "" });
  const {
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isDirty: venueIsDirty, isValid },
  } = useForm<UpdateVenueRequest>({
    resolver: zodResolver(updateVenueRequestSchema),
    defaultValues: emptyRequest,
    mode: "onChange",
  });

  const mutation = useMutation({
    mutationFn: async (request: UpdateVenueRequest) => {
      const saved = await venueApi.updateVenue(request);
      if (options?.afterSave) await options.afterSave();
      return saved;
    },
    onSuccess: (saved) => {
      queryClient.setQueryData(venueKeys.my(), saved);
      queryClient.setQueryData(venueKeys.byId(saved.id), saved);
      reset(Venue.toUpdateRequest(saved));
      setLocationDisplay({ county: saved.county, town: saved.town });
      setEditMode(false);
      options?.onSuccess?.(saved);
    },
  });

  const venue = query.data ?? undefined;
  const request = watch();
  const draft =
    editMode && venue
      ? {
          ...venue,
          ...request,
          ...location,
          bannerUrl: request.banner?.uri ?? venue.bannerUrl,
          avatar: request.avatar?.uri ?? venue.avatar,
        }
      : undefined;

  const resetForm = () => {
    if (venue) {
      reset(Venue.toUpdateRequest(venue));
      setLocationDisplay({ county: venue.county, town: venue.town });
    }
    setEditMode(false);
  };

  const resetDraft = () => {
    resetForm();
    options?.onResetDraft?.();
  };

  const toggleEdit = () => {
    if (editMode) {
      resetForm();
    } else if (venue) {
      reset(Venue.toUpdateRequest(venue));
      setLocationDisplay({ county: venue.county, town: venue.town });
      setEditMode(true);
    }
    options?.onToggleEdit?.();
  };

  const save = () => {
    void handleSubmit((request) => mutation.mutate(request))();
  };

  const saveError = venueIsDirty
    ? errors.name?.message ??
      errors.about?.message ??
      errors.latitude?.message ??
      errors.longitude?.message ??
      errors.banner?.message ??
      errors.avatar?.message
    : undefined;

  return {
    venue,
    draft,
    isLoading: query.isLoading,
    isError: query.isError,
    editMode,
    isDirty: venueIsDirty || (options?.extraDirty ?? false),
    isSaving: mutation.isPending,
    canSave: isValid,
    saveError,
    save,
    toggleEdit,
    resetDraft,
    setName: (name) =>
      setValue("name", name, { shouldDirty: true, shouldValidate: true }),
    setAbout: (about) =>
      setValue("about", about, { shouldDirty: true, shouldValidate: true }),
    setBanner: (banner) =>
      setValue("banner", banner, { shouldDirty: true, shouldValidate: true }),
    setAvatar: (avatar) =>
      setValue("avatar", avatar, { shouldDirty: true, shouldValidate: true }),
    setLocation: (latitude, longitude, county, town) => {
      setValue("latitude", latitude, {
        shouldDirty: true,
        shouldValidate: true,
      });
      setValue("longitude", longitude, {
        shouldDirty: true,
        shouldValidate: true,
      });
      setLocationDisplay({ county, town });
    },
  };
}
