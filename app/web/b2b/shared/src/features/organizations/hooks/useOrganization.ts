import { toast } from "sonner";
import { useOrganizationQuery } from "./useOrganizationQuery";
import { useUpdateOrganizationMutation } from "./useUpdateOrganizationMutation";
import { updateOrganizationRequestSchema } from "../schemas/updateOrganizationRequestSchema";

export interface OrganizationBuffer {
  legalName: string;
  vatRegistered: boolean;
  vatNumber: string;
  sellerIdentifier: string;
  line1: string;
  line2: string;
  city: string;
  postcode: string;
  country: string;
  bankReference: string;
  holdsMusicLicence: boolean;
  prsPassThroughPercent: string;
  vatPercent: string;
  paymentTermsDays: string;
  cancellationNoticeHours: string;
}

export function useOrganization() {
  const { data: organization, isLoading } = useOrganizationQuery();
  const { mutate, isPending } = useUpdateOrganizationMutation();

  const save = (buffer: OrganizationBuffer) => {
    const parsed = updateOrganizationRequestSchema.safeParse({
      legalName: buffer.legalName,
      taxCompliance: {
        vatNumber: buffer.vatRegistered ? buffer.vatNumber : undefined,
        sellerIdentifier: buffer.sellerIdentifier,
        registeredAddress: {
          line1: buffer.line1,
          line2: buffer.line2 || undefined,
          city: buffer.city,
          postcode: buffer.postcode,
          country: buffer.country,
        },
        bankReference: buffer.bankReference,
        holdsMusicLicence: buffer.holdsMusicLicence,
      },
      configuration: {
        prsPassThroughRate: buffer.prsPassThroughPercent
          ? Number(buffer.prsPassThroughPercent) / 100
          : undefined,
        vatRate: buffer.vatPercent ? Number(buffer.vatPercent) / 100 : undefined,
        paymentTermsDays: buffer.paymentTermsDays
          ? Number(buffer.paymentTermsDays)
          : undefined,
        cancellationNoticeHours: buffer.cancellationNoticeHours
          ? Number(buffer.cancellationNoticeHours)
          : undefined,
      },
    });
    if (parsed.success)
      mutate(parsed.data, { onSuccess: () => toast.success("Details saved") });
    return parsed;
  };

  return { organization, isLoading, isSaving: isPending, save };
}
