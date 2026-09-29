import Image from "next/image";
import Link from "next/link";

/** Current AUB seal asset (black & white). Filename change busts caches. */
export const LOGO_SRC = "/aub-logo.png";

/** White AUB seal used on the sign-in button. */
export const AUB_STAMP_SRC = "/AUB_Stamp.png";

/** White institute lockup used in the site navbar. */
export const NAV_LOGO_SRC = "/IAID_URVO_LogoWhite.png";

/** White AUB + institute lockup used in the site footer. */
export const FOOTER_LOGO_SRC =
  "/_Institute%20for%20Academic%20Innovation%20and%20Development%20AUB%20Lockups_1.1-White.png";

/** Horizontal Student Success Unit mark. */
export const SSU_LOGO_SRC = "/SSU%20Logo%20horizontal.png";

type LogoProps = {
  href?: string;
  /** Display height in pixels. */
  size?: number;
  /**
   * Display width in pixels for a wide lockup. Omit for the square seal.
   * When set, height follows `size` and width scales with the image.
   */
  width?: number;
  src?: string;
  alt?: string;
  showWordmark?: boolean;
  /** Replaces the fixed pixel size, for responsive lockups. */
  imageClassName?: string;
  className?: string;
  onClick?: () => void;
};

export function Logo({
  href = "/",
  size = 40,
  width,
  src = LOGO_SRC,
  alt = "American University of Beirut",
  showWordmark = true,
  imageClassName,
  className = "",
  onClick,
}: LogoProps) {
  const content = (
    <>
      <Image
        src={src}
        alt={alt}
        width={width ?? size}
        height={size}
        className={`object-contain ${imageClassName ?? ""}`}
        style={
          imageClassName
            ? undefined
            : width
              ? { height: `${size / 16}rem`, width: "auto" }
              : { width: `${size / 16}rem`, height: `${size / 16}rem` }
        }
        priority
        unoptimized
      />
      {showWordmark ? (
        <span className="font-[family-name:var(--font-display)] text-2xl font-semibold tracking-tight">
          URVP
        </span>
      ) : null}
    </>
  );

  if (!href) {
    return (
      <span className={`inline-flex items-center gap-3 ${className}`}>
        {content}
      </span>
    );
  }

  return (
    <Link
      href={href}
      onClick={onClick}
      className={`inline-flex items-center gap-3 ${className}`}
    >
      {content}
    </Link>
  );
}
