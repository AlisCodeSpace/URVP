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
  const [frame, setFrame] = useState({ images, index: 0 });
  if (frame.images !== images) {
    setFrame({ images, index: 0 });
  }
  const index = frame.images === images ? frame.index : 0;
  const count = images.length;

  const go = useCallback(
    (direction: -1 | 1) => {
      if (count <= 1) return;
      setFrame((current) => {
        const currentIndex = current.images === images ? current.index : 0;
        return {
          images,
          index: (currentIndex + direction + count) % count,
        };
      });
    },
    [count, images],
  );

  if (count === 0) return null;

  const current = images[index] ?? images[0];
  const label = count > 1 ? `${alt} (${index + 1} of ${count})` : alt;

  return (
    <div className="news-image-slider">
      <div className="news-image-slider-frame">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={current} alt={label} />
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
              key={images[i]}
              type="button"
              role="tab"
              aria-selected={i === index}
              aria-label={`Show image ${i + 1}`}
              className={i === index ? "is-active" : undefined}
              onClick={() =>
                setFrame({
                  images,
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
