import Image from "next/image";
import Link from "next/link";

/** Current AUB seal asset (black & white). Filename change busts caches. */
export const LOGO_SRC = "/aub-logo.png";

type LogoProps = {
  href?: string;
  size?: number;
  showWordmark?: boolean;
  className?: string;
  onClick?: () => void;
};

export function Logo({
  href = "/",
  size = 40,
  showWordmark = true,
  className = "",
  onClick,
}: LogoProps) {
  const content = (
    <>
      <Image
        src={LOGO_SRC}
        alt="American University of Beirut"
        width={size}
        height={size}
        className="object-contain"
        style={{ width: size, height: size }}
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
