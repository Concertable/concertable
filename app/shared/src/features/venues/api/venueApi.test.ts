import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import venueApi from "./venueApi";

const mocks = vi.hoisted(() => ({
  post: vi.fn(),
  put: vi.fn(),
}));

vi.mock("../../../lib/apiClient", () => ({
  apiClient: {
    post: mocks.post,
    put: mocks.put,
  },
}));

class CapturingFormData {
  readonly fields: Array<[string, unknown]> = [];

  append(name: string, value: unknown) {
    this.fields.push([name, value]);
  }
}

describe("venueApi", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal("FormData", CapturingFormData);
  });

  afterEach(() => vi.unstubAllGlobals());

  it("encodes every create field with the multipart contract names", async () => {
    const banner = { uri: "banner", name: "banner.jpg", type: "image/jpeg" };
    const avatar = { uri: "avatar", name: "avatar.jpg", type: "image/jpeg" };
    mocks.post.mockResolvedValue({ data: { id: 42 } });

    await venueApi.createVenue({
      name: "Example Venue",
      about: "About",
      latitude: 51.5,
      longitude: -0.1,
      banner,
      avatar,
    });

    const formData = mocks.post.mock.calls[0][1] as CapturingFormData;
    expect(mocks.post).toHaveBeenCalledWith("/organization/venue", formData);
    expect(formData.fields).toEqual([
      ["Name", "Example Venue"],
      ["About", "About"],
      ["Latitude", "51.5"],
      ["Longitude", "-0.1"],
      ["Banner", banner],
      ["Avatar", avatar],
    ]);
  });

  it("encodes replaced images and omits unchanged images from an update", async () => {
    const banner = { uri: "banner", name: "banner.jpg", type: "image/jpeg" };
    mocks.put.mockResolvedValue({ data: { id: 42 } });

    await venueApi.updateVenue({
      name: "Example Venue",
      about: "About",
      latitude: 51.5,
      longitude: -0.1,
      banner,
    });

    const formData = mocks.put.mock.calls[0][1] as CapturingFormData;
    expect(mocks.put).toHaveBeenCalledWith("/organization/venue", formData);
    expect(formData.fields).toEqual([
      ["Name", "Example Venue"],
      ["About", "About"],
      ["Latitude", "51.5"],
      ["Longitude", "-0.1"],
      ["Banner", banner],
    ]);
  });
});
