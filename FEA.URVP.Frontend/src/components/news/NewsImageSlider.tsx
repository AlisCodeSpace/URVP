"use client";

import { useCallback, useEffect, useState } from "react";
import { IconChevronLeft, IconChevronRight } from "@/components/ui/Icons";

export function NewsImageSlider({
  images,
  alt,
}: {
  images: string[];
  alt: string;
}) {
  const [index, setIndex] = useState(0);
  const count = images.length;

  const go = useCallback(
    (direction: -1 | 1) => {
      if (count <= 1) return;
      setIndex((current) => (current + direction + count) % count);
    },
    [count],
  );

  useEffect(() => {
    setIndex(0);
  }, [images]);

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
              onClick={() => setIndex(i)}
            />
          ))}
        </div>
      ) : null}
    </div>
  );
}
