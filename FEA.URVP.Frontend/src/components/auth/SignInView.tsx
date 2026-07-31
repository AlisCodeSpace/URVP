import Image from "next/image";
import Link from "next/link";
import { Heading, Text } from "@radix-ui/themes";
import { LOGO_SRC } from "@/components/ui/Logo";
import { authErrorMessage } from "@/lib/auth";

type SignInViewProps = {
  signInUrl: string;
  error?: string;
};

export function SignInView({ signInUrl, error }: SignInViewProps) {
  const errorText = authErrorMessage(error);

  return (
    <main className="sign-in-shell flex h-dvh min-h-0 flex-1 flex-col lg:flex-row">
      <section className="sign-in-brand relative flex min-h-0 flex-[0.48] flex-col overflow-hidden px-7 py-7 sm:px-8 lg:flex-[0.52] lg:px-12 lg:py-9 xl:flex-[0.54] xl:px-14">
        <div className="sign-in-brand-grid absolute inset-0" aria-hidden />
        <div
          className="sign-in-glow pointer-events-none absolute -left-16 top-1/3 h-64 w-64 rounded-full bg-secondary/20 blur-3xl"
          aria-hidden
        />

        <p className="sign-in-enter relative z-10 shrink-0 text-xs font-medium uppercase tracking-[0.28em] text-secondary">
          AY 2025–26
        </p>

        <div className="relative z-10 flex flex-1 flex-col justify-center py-5 lg:py-6">
          <p
            className="sign-in-watermark relative -ml-1 select-none lg:-ml-2"
            aria-hidden
          >
            URVP
          </p>
          <Heading
            as="h2"
            size="8"
            weight="medium"
            my="4"
            className="sign-in-enter-delay relative max-w-lg !font-[family-name:var(--font-display)] !leading-[1.05] !text-white"
          >
            Where faculty projects meet student ambition.
          </Heading>
          <Text
            as="p"
            size="3"
            className="sign-in-enter-delay relative max-w-md !leading-relaxed !text-white/72"
          >
            Sign in with your AUB account to browse research opportunities,
            manage your profile, and connect with mentors across campus.
          </Text>
          <ul className="sign-in-enter-delay relative mt-5 space-y-2 border-l-2 border-secondary/80 pl-4 text-sm text-white/65">
            <li>800+ students matched since 2019</li>
            <li>Projects across all AUB faculties</li>
            <li>Office of the Provost initiative</li>
          </ul>
        </div>

        <Text
          as="p"
          size="1"
          className="sign-in-enter relative z-10 shrink-0 !uppercase !tracking-[0.2em] !text-white/40"
        >
          American University of Beirut
        </Text>
      </section>

      <section className="flex min-h-0 flex-1 items-center justify-center px-6 py-7 sm:px-8 lg:px-10">
        <div className="sign-in-enter w-full max-w-[24rem]">
          <div className="sign-in-card overflow-hidden">
            <div className="sign-in-accent" aria-hidden />
            <div className="px-7 py-7 sm:px-8 sm:py-8">
              <Text
                as="p"
                size="2"
                weight="medium"
                className="!uppercase !tracking-[0.22em] !text-secondary-deep"
              >
                Portal access
              </Text>
              <Heading
                as="h1"
                size="7"
                weight="medium"
                mt="2"
                className="!font-[family-name:var(--font-display)] !text-primary"
              >
                Sign in
              </Heading>

              {errorText ? (
                <Text
                  as="p"
                  size="2"
                  mt="4"
                  role="alert"
                  className="rounded-md bg-red-50 px-3 py-2 !leading-relaxed !text-red-800"
                >
                  {errorText}
                </Text>
              ) : null}

              <a
                href={signInUrl}
                className="btn btn-primary btn-lg mt-6 w-full gap-3"
                aria-label="Sign in with AUB"
              >
                <Image
                  src={LOGO_SRC}
                  alt=""
                  width={26}
                  height={26}
                  className="h-[26px] w-[26px] object-contain"
                  unoptimized
                />
                Continue with AUB
              </a>

              <Text
                as="p"
                size="1"
                mt="3"
                className="text-center !leading-relaxed !text-muted/80"
              >
                Secured by AUB single sign-on. You will be redirected to the
                official login page.
              </Text>
            </div>
          </div>

          <div className="mt-5 flex justify-center">
            <Link href="/" className="sign-in-back">
              <span aria-hidden className="text-base leading-none">
                ←
              </span>
              Back to home
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
