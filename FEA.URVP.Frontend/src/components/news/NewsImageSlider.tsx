"use client";

import { useCallback, useState } from "react";
import { IconChevronLeft, IconChevronRight } from "@/components/ui/Icons";

export function NewsImageSlider({
  images,
  alt,
}: {
  images: string[];
  alt: string;
}) {
  const signature = images.join("\n");
  const [frame, setFrame] = useState({ signature, index: 0 });
  if (frame.signature !== signature) {
    setFrame({ signature, index: 0 });
  }
  const index =
    frame.signature === signature
      ? Math.min(frame.index, Math.max(images.length - 1, 0))
      : 0;
  const count = images.length;

  const go = useCallback(
    (direction: -1 | 1) => {
      if (count <= 1) return;
      setFrame((current) => {
        const currentIndex = current.signature === signature ? current.index : 0;
        return {
          signature,
          index: (currentIndex + direction + count) % count,
        };
      });
    },
    [count, signature],
  );

  if (count === 0) return null;

  const current = images[index] ?? images[0];
  const label = count > 1 ? `${alt} (${index + 1} of ${count})` : alt;

  return (
    <div className="news-image-slider">
      <div className="news-image-slider-frame">
        {/* A fresh element per slide. Reusing one <img> kept painting the first photo after src changed. */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img key={`${index}:${current}`} src={current} alt={label} />
        {count > 1 ? (
          <>
            <button
              type="button"
              className="news-image-slider-nav is-prev"
              aria-label="Previous image"
              onClick={() => go(-1)}
            >
              <IconChevronLeft size={20} />
            </button>
            <button
              type="button"
              className="news-image-slider-nav is-next"
              aria-label="Next image"
              onClick={() => go(1)}
            >
              <IconChevronRight size={20} />
            </button>
          </>
        ) : null}
      </div>
      {count > 1 ? (
        <div className="news-image-slider-dots" role="tablist" aria-label="Story images">
          {images.map((_, i) => (
            <button
              key={`${i}:${images[i]}`}
              type="button"
              role="tab"
              aria-selected={i === index}
              aria-label={`Show image ${i + 1}`}
              className={i === index ? "is-active" : undefined}
              onClick={() =>
                setFrame({
                  signature,
                  index: i,
                })
              }
            />
          ))}
        </div>
      ) : null}
    </div>
  );
}
