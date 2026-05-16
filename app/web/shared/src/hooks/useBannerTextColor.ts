import { useState, useEffect } from "react";

const LUMINANCE_WEIGHTS = {
  r: 0.299,
  g: 0.587,
  b: 0.114,
};

const SAMPLE_REGION = {
  widthRatio: 0.5,
  heightRatio: 0.4,
  alignBottom: true,
};

const SAMPLE_SIZE = 20;

function getAverageLuminance(src: string): Promise<number> {
  return new Promise((resolve) => {
    const img = new Image();
    img.crossOrigin = "anonymous";

    img.onload = () => {
      const canvas = document.createElement("canvas");
      canvas.width = SAMPLE_SIZE;
      canvas.height = SAMPLE_SIZE;

      const ctx = canvas.getContext("2d");
      if (!ctx) return resolve(0);

      const sampleW = img.width * SAMPLE_REGION.widthRatio;
      const sampleH = img.height * SAMPLE_REGION.heightRatio;
      const sampleY = SAMPLE_REGION.alignBottom ? img.height - sampleH : 0;

      ctx.drawImage(
        img,
        0,
        sampleY,
        sampleW,
        sampleH,
        0,
        0,
        SAMPLE_SIZE,
        SAMPLE_SIZE,
      );

      const { data } = ctx.getImageData(0, 0, SAMPLE_SIZE, SAMPLE_SIZE);

      let total = 0;

      for (let i = 0; i < data.length; i += 4) {
        total +=
          LUMINANCE_WEIGHTS.r * data[i] +
          LUMINANCE_WEIGHTS.g * data[i + 1] +
          LUMINANCE_WEIGHTS.b * data[i + 2];
      }

      resolve(total / (data.length / 4));
    };

    img.onerror = () => resolve(0);
    img.src = src;
  });
}

function getTextColor(luminance: number): "white" | "black" {
  const contrastWithWhite = 255 - luminance;
  const contrastWithBlack = luminance;
  return contrastWithWhite > contrastWithBlack ? "white" : "black";
}

export function useBannerTextColor(src?: string) {
  const [textColor, setTextColor] = useState<"white" | "black" | null>(null);

  useEffect(() => {
    if (!src) return;

    let cancelled = false;

    getAverageLuminance(src).then((luminance) => {
      if (!cancelled) {
        setTextColor(getTextColor(luminance));
      }
    });

    return () => {
      cancelled = true;
    };
  }, [src]);

  return textColor;
}
