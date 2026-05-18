import api from "../lib/axiosClient";

const blobApi = {
  download: async (fileName: string): Promise<string> => {
    const response = await api.get(`/blob/download/${fileName}`, {
      responseType: "arraybuffer",
    });
    const bytes = new Uint8Array(response.data as ArrayBuffer);
    const chunkSize = 8192;
    let binary = "";
    for (let i = 0; i < bytes.length; i += chunkSize)
      binary += String.fromCharCode(...Array.from(bytes.subarray(i, i + chunkSize)));
    const contentType = (response.headers["content-type"] as string | undefined) ?? "image/jpeg";
    return `data:${contentType};base64,${btoa(binary)}`;
  },
};

export default blobApi;
