import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import type { ImageFile } from "../../../types/image";
import artistApi from "../api/artistApi";
import { updateArtistRequestSchema } from "../schemas/artistRequestSchemas";
import { Artist, type UpdateArtistRequest } from "../types";
import { artistKeys, useMyArtistQuery } from "./useArtistQuery";

export interface UseMyArtistOptions {
  onSuccess?: (saved: Artist) => void;
}

export interface UseMyArtistResult {
  artist: Artist | undefined;
  draft: Artist | undefined;
  isLoading: boolean;
  isError: boolean;
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
}

const emptyRequest: UpdateArtistRequest = {
  name: "",
  about: "",
  latitude: 0,
  longitude: 0,
  genres: [],
};

export function useMyArtist(options?: UseMyArtistOptions): UseMyArtistResult {
  const query = useMyArtistQuery();
  const queryClient = useQueryClient();
  const [editMode, setEditMode] = useState(false);
  const {
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isDirty, isValid },
  } = useForm<UpdateArtistRequest>({
    resolver: zodResolver(updateArtistRequestSchema),
    defaultValues: emptyRequest,
    mode: "onChange",
  });

  const mutation = useMutation({
    mutationFn: artistApi.updateArtist,
    onSuccess: (saved) => {
      queryClient.setQueryData(artistKeys.my(), saved);
      queryClient.setQueryData(artistKeys.byId(saved.id), saved);
      reset(Artist.toUpdateRequest(saved));
      setEditMode(false);
      options?.onSuccess?.(saved);
    },
  });

  const artist = query.data ?? undefined;
  const request = watch();
  const draft =
    editMode && artist
      ? {
          ...artist,
          ...request,
          bannerUrl: request.banner?.uri ?? artist.bannerUrl,
          avatar: request.avatar?.uri ?? artist.avatar,
        }
      : undefined;

  const resetDraft = () => {
    if (artist) reset(Artist.toUpdateRequest(artist));
    setEditMode(false);
  };

  const toggleEdit = () => {
    if (editMode) {
      resetDraft();
      return;
    }

    if (artist) {
      reset(Artist.toUpdateRequest(artist));
      setEditMode(true);
    }
  };

  const save = () => {
    void handleSubmit((request) => mutation.mutate(request))();
  };

  const saveError = isDirty
    ? errors.name?.message ??
      errors.about?.message ??
      errors.latitude?.message ??
      errors.longitude?.message ??
      errors.genres?.message ??
      errors.banner?.message ??
      errors.avatar?.message
    : undefined;

  return {
    artist,
    draft,
    isLoading: query.isLoading,
    isError: query.isError,
    editMode,
    isDirty,
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
  };
}
